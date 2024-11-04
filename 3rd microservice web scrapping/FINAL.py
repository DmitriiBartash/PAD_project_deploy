import sys, time, requests, json
import asyncio, aiohttp
from aiohttp import TCPConnector
from selectolax.parser import HTMLParser
from flask import Flask
import hashlib


class WebScr_Conditionere:
    baseUrl = "https://conditionere.md/ru/nastennye-kondicionery/?page="
    products = []
    page = 2

    # fix of dumb aiohttp issue, smt doesnt work on windows
    if sys.platform == "win32":
        asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())

    async def fetch(self, url, session):
        """Fetch HTML content for a given URL."""
        try:
            async with session.get(url, ssl=False) as response:
                return await response.text()
        except Exception as e:
            print(f"Error fetching {url}: {e}")
            return None

    # New function for processing the HTML data
    async def process_page(self, html_content):
        try:
            parser = HTMLParser(html_content)
            productsItems = parser.css("div.prods_row > div")

            # Gather tasks to process each product card
            getProductTasks = [
                asyncio.create_task(self.GetProducts(product))
                for product in productsItems
            ]
            product_details = await asyncio.gather(*getProductTasks)
            return product_details
        except Exception as e:
            print(f"Error in process_page: {e}")
            return []

    async def fetch_all_pages(self, urls):
        """Fetch and process all pages concurrently."""
        results = []
        connector = TCPConnector(limit=100)
        try:
            async with aiohttp.ClientSession(connector=connector) as session:
                fetch_tasks = [self.fetch(url, session) for url in urls]
                pages_content = await asyncio.gather(*fetch_tasks)

                process_tasks = [self.process_page(html) for html in pages_content]
                page_results = await asyncio.gather(*process_tasks)

                for result in page_results:
                    results.extend(result)
        except Exception as e:
            print(f"Error in fetch_all_pages: {e}")
        return results

    async def get_initial_page_data(self):
        """Fetch the first page to determine the total number of pages and get the products on the first page."""
        connector = TCPConnector(limit=100)
        async with aiohttp.ClientSession(connector=connector) as session:
            url = f"{self.baseUrl}1"
            page_content = await self.fetch(url, session)

            first_page_products = await self.process_page(page_content)

            # Extract products from the first page
            parser = HTMLParser(page_content)

            # Find the last page number
            page_links = parser.css("a.pagelink")
            last_page = int(page_links[-1].text(strip=True)) if page_links else 1

            return first_page_products, last_page

    async def GetProducts(self, product_card):
        try:
            _name = product_card.css_first("a.prod_card_title").text(strip=True)
            _link = product_card.css_first("a.prod_card_title").attributes["href"]
            _price = product_card.css_first("div.prod_card_price").text(strip=True)
            _pcp_rows = product_card.css("div.pcp_row")
            _BTU = _pcp_rows[1].css_first("div.pcp_value").text(strip=True)
            _serviceArea = _pcp_rows[2].css_first("div.pcp_value").text(strip=True)

            return {
                "url": "https://conditionere.md" + _link,
                "name": _name,
                "price": _price,
                "btu": _BTU,
                "serviceArea": _serviceArea,
            }
        except Exception as e:
            print(f"Error in GetProducts:{e}")

    async def GetConditionere(self):
        # get lastPage initially
        initial_products, last_page = await self.get_initial_page_data()
        # get urls
        urls = [f"{self.baseUrl}{i}" for i in range(self.page, last_page + 1)]
        # Fetch and process remaining pages
        remaining_products = await self.fetch_all_pages(urls)
        # Combine products from all pages
        return initial_products + remaining_products


class WebScr_Darwin:
    baseUrl = "https://darwin.md/ru/klimaticheskaja-tehnika/konditsionery?page="
    # open("conditionere.md/data.json", "w").close()
    products = []
    page = 2

    # fix of dumb aiohttp issue, smt doesnt work on windows
    if sys.platform == "win32":
        asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())

    async def fetch(self, url, session):
        """Fetch HTML content for a given URL."""
        try:
            async with session.get(url, ssl=False) as response:
                return await response.text()
        except Exception as e:
            print(f"Error fetching {url}: {e}")
            return None

    # New function for processing the HTML data
    async def process_page(self, html_content, session, url):
        try:
            parser = HTMLParser(html_content)
            productsItems = parser.css("div.product-card")

            # Gather tasks to process each product card
            getProductTasks = [
                asyncio.create_task(self.GetProducts(product, session, url))
                for product in productsItems
            ]
            product_details = await asyncio.gather(*getProductTasks)
            return product_details

        except Exception as e:
            print(f"Error in process_page: {e}")
            return []

    async def fetch_all_pages(self, urls):
        """Fetch and process all pages concurrently."""
        results = []
        connector = TCPConnector(limit=100)
        try:
            async with aiohttp.ClientSession(connector=connector) as session:
                fetch_tasks = [self.fetch(url, session) for url in urls]
                pages_content = await asyncio.gather(*fetch_tasks)

                process_tasks = [
                    self.process_page(html, session, url)
                    for html, url in zip(pages_content, urls)
                ]
                page_results = await asyncio.gather(*process_tasks)

                for result in page_results:
                    newArr = result
                    smt = []
                    for item in newArr:
                        if item != None:
                            smt.append(item)

                    if smt != []:
                        results.extend(smt)
        except Exception as e:
            print(f"Error in fetch_all_pages: {e}")
        return results

    async def get_initial_page_data(self):
        """Fetch the first page to determine the total number of pages and get the products on the first page."""
        connector = TCPConnector(limit=100)
        async with aiohttp.ClientSession(connector=connector) as session:
            url = f"{self.baseUrl}1"
            page_content = await self.fetch(url, session)

            first_page_products = await self.process_page(page_content, session, url)

            # Extract products from the first page
            parser = HTMLParser(page_content)

            # Find the last page number
            page_links = parser.css("ul.pagination")
            li_elements = page_links[0].css("li")

            pageNumbers = []
            for li in li_elements:
                a_tag = li.css_first("a")
                if a_tag:
                    try:
                        pageNumbers.append(int(a_tag.text(strip=True)))
                    except:
                        print("error parsing")
            last_page = max(pageNumbers)

            return first_page_products, last_page

    async def GetProducts(self, product_card, session, url):
        try:
            _name = product_card.css_first("div.title-product").text(strip=True)
            _link = product_card.css_first("a.product-link").attributes["href"]
            _price = product_card.css_first("div.price-new").text(strip=True)
            _btu = ""
            _serviceArea = ""

            if "РќРµРґРѕСЃС‚СѓРїРЅРѕ" not in _price:
                async with session.get(_link, ssl=False) as response:
                    _product_data = await response.text()
                    _product_soup = HTMLParser(_product_data)

                    # Find all <td> elements
                    _tds = _product_soup.css("td")
                    index = 0

                    for _td in _tds:
                        if _td.text(strip=True) == "РњРѕС‰РЅРѕСЃС‚СЊ Р‘РўР•":
                            break
                        index += 1

                    if index != len(_tds):
                        _btu = _tds[index + 1].text(strip=True)
                        if (
                            index + 2 < len(_tds)
                            and _tds[index + 2].text(strip=True)
                            == "РћР±СЃР»СѓР¶РёРІР°РµРјР°СЏ РїР»РѕС‰Р°РґСЊ"
                        ):
                            _serviceArea = _tds[index + 3].text(strip=True)
                            if "Рј2" in _serviceArea:
                                _serviceArea = _serviceArea.replace("Рј2", "")
                            elif "РјВІ" in _serviceArea:
                                _serviceArea = _serviceArea.replace("РјВІ", "")
                            _serviceArea = _serviceArea.strip()

                            return {
                                "url": _link,
                                "name": _name,
                                "price": _price,
                                "btu": _btu,
                                "serviceArea": _serviceArea,
                            }
                # await asyncio.sleep(0)
        except Exception as e:
            print(f"Error in GetProducts:{e}")

    async def GetConditionere(self):
        # get lastPage initially
        initial_products, last_page = await self.get_initial_page_data()
        # get urls
        urls = [f"{self.baseUrl}{i}" for i in range(self.page, last_page + 1)]
        # Fetch and process remaining pages
        remaining_products = await self.fetch_all_pages(urls)
        # Combine products from all pages
        return initial_products + remaining_products


app = Flask(__name__)


@app.route("/")
def hello_world():
    return "<p>Hello, World! It's the scrapping</p>"


@app.route("/api/cond")
def getCond():
    cond = WebScr_Conditionere()
    data = asyncio.run(cond.GetConditionere())


    # cond1 = WebScr_Darwin()
    # data.extend(asyncio.run(cond1.GetConditionere()))

    converted = str(data).encode("utf-8")
    h = hashlib.new("sha256")
    h.update(converted)

    return {"hash": h.hexdigest(), "items": data}
    # test size
    # return str(sys.getsizeof(data))
    # return h.hexdigest()

    # test length
    # print(len(data))


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)


# cond1 = WebScr_Darwin()
# data1 = asyncio.run(cond1.GetConditionere())
# print(len(data1))