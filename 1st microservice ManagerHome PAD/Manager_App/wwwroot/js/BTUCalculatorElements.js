// Additional params
function toggleParams() {
    const additionalParams = document.getElementById('additionalParams');
    additionalParams.style.display = (additionalParams.style.display === 'none' || additionalParams.style.display === '') ? 'block' : 'none';
}

function toggleVentilation() {
    const hasVentilation = document.getElementById('hasVentilation').value;
    const airExchangeRateGroup = document.getElementById('airExchangeRateGroup');
    airExchangeRateGroup.style.display = (hasVentilation === 'true') ? 'block' : 'none';
}

function toggleWindowArea() {
    const hasLargeWindow = document.getElementById('hasLargeWindow').value;
    const windowAreaGroup = document.getElementById('windowAreaGroup');
    windowAreaGroup.style.display = (hasLargeWindow === 'true') ? 'block' : 'none';
}

function replaceCommaWithDot() {
    const inputs = document.querySelectorAll("input[type='text'], input[type='number']");
    inputs.forEach(input => {
        input.addEventListener('input', (event) => {
            event.target.value = event.target.value.replace(/,/g, '.');
        });
    });
}

function setDefaultValues() {
    const inputs = document.querySelectorAll("input[type='text'], input[type='number']");
    inputs.forEach(input => {
        input.value = '0';
    });
}

document.addEventListener('DOMContentLoaded', () => {
    setDefaultValues(); 
    replaceCommaWithDot();
});

function clearForm() {
    const form = document.getElementById("btuCalculatorForm");
    form.reset();

    const inputs = form.querySelectorAll("input[type='text'], input[type='number']");
    inputs.forEach(input => {
        input.value = '0';
    });

    const validationErrors = document.querySelector(".validation-summary-errors");
    if (validationErrors) {
        validationErrors.style.display = "none";
    }

    const resultsContainer = document.getElementById('calculationResults');
    resultsContainer.innerHTML = '';
    const btuResultContainer = document.getElementById('btuResult');
    btuResultContainer.style.display = 'none';

    const additionalParams = document.getElementById('additionalParams');
    additionalParams.style.display = 'none';

    const conditionerTableBody = document.querySelector('.conditioner-form tbody');
    if (conditionerTableBody) {
        conditionerTableBody.innerHTML = ''; 
    }
}

let lowerBTU;
let upperBTU;
function calculateBTU() {
    const formData = new FormData(document.getElementById('btuCalculatorForm'));
    const jsonData = {};

    formData.forEach((value, key) => {
        let newKey = key.replace("RequestModel.", "");
        let newValue;
        // console.log(key, ":", value);
        if (newKey == "PeopleCount" || newKey == "NumberOfComputers" || newKey == "NumberOfTVs" || newKey == "RoomSize" || newKey == "CeilingHeight" || newKey == "OtherAppliancesKWattage") {
            newValue = Number(value);
        } else if (newKey == "HasVentilation" || newKey == "Guaranteed20Degrees" || newKey == "IsTopFloor" || newKey == "HasLargeWindow") {
            newValue = value === "true";
        } else if (newKey == "AirExchangeRate" || newKey == "WindowArea") {
            newValue = Number(value);
        } else {
            newValue = value;
        }
        jsonData[newKey] = newValue;
    });
    // console.log("Отправляемые данные:", jsonData);

    if (!jsonData['SizeUnit'] || !jsonData['HeightUnit'] || !jsonData['SunExposure']) {
        console.error("Обязательные поля не заполнены:", jsonData);
        alert("Пожалуйста, заполните все обязательные поля."); 
        return; 
    }

    // Логируем данные перед отправкой
    // console.log("Отправляемые данные:", jsonData);

    $.ajax({
        url: '/CalculateBTU/CalculateBtuValue', 
        type: 'POST',
        data: JSON.stringify(jsonData), 
        contentType: 'application/json', 
        success: function (data) {
            lowerBTU = data.recommendedRangeBTU.lower;
            upperBTU = data.recommendedRangeBTU.upper;
            // console.log("Границы BTU:", lowerBTU, upperBTU);

            /// console.log("Данные, полученные от сервера:", data); 
            const resultsContainer = document.getElementById('calculationResults');
            resultsContainer.innerHTML = `
                <div class="btu-result-item-btu">
                    <span class="btu-label-btu">Calculated Power (BTU):</span>
                    <span class="btu-value-btu">${data.calculatedPowerBTU}</span>
                </div>
                <div class="btu-result-item-btu">
                    <span class="btu-label-btu">Recommended Range (BTU):</span>
                    <span class="btu-value-btu">${data.recommendedRangeBTU.lower} - ${data.recommendedRangeBTU.upper}</span>
                </div>
                <div class="btu-result-item-kw">
                    <span class="btu-label-kw">Calculated Power (kW):</span>
                    <span class="btu-value-kw">${data.calculatedPowerKW}</span>
                </div>
                <div class="btu-result-item-kw">
                    <span class="btu-label-kw">Recommended Range (kW):</span>
                    <span class="btu-value-kw">${data.recommendedRangeKW.lower} - ${data.recommendedRangeKW.upper}</span>
                </div>
            `;
            const btuResultContainer = document.getElementById('btuResult');
            btuResultContainer.style.display = 'block';
        },
        error: function (xhr, status, error) {
            console.error('Ошибка AJAX:', xhr.responseText); 
        }
    });
}

function updateConditioners() {
    $.ajax({
        url: '/CalculateBTU/UpdateConditioners',
        type: 'GET',
        success: function (data) {
            console.log("Данные, полученные от сервера:", data);
            const conditionerTableBody = document.querySelector('.conditioner-form tbody');
            conditionerTableBody.innerHTML = ''; 

            const noDataMessage = document.getElementById('noDataMessage');
            if (data && data.length > 0) {
                noDataMessage.style.display = 'none'; 

                const btuValues = data.map(conditioner => parseInt(conditioner.btu, 10)); 
                const minBtu = Math.min(...btuValues); // Минимальное значение BTU
                const maxBtu = Math.max(...btuValues); // Максимальное значение BTU

                let filteredConditioners;
                if (upperBTU < minBtu) {
                    // Если upperBTU меньше минимального значения BTU, выводим кондиционеры с минимальным BTU
                    filteredConditioners = data.filter(conditioner => parseInt(conditioner.btu, 10) === minBtu);
                } else if (lowerBTU > maxBtu) {
                    // Если lowerBTU больше максимального значения BTU, выводим кондиционеры с максимальным BTU
                    filteredConditioners = data.filter(conditioner => parseInt(conditioner.btu, 10) === maxBtu);
                } else {
                    // В противном случае, выводим кондиционеры в заданном диапазоне
                    filteredConditioners = data.filter(conditioner => {
                        const btuValue = parseInt(conditioner.btu, 10); 
                        return btuValue >= lowerBTU && btuValue <= upperBTU; 
                    });
                }

                // Сортируем кондиционеры в порядке возрастания BTU
                filteredConditioners.sort((a, b) => {
                    return parseInt(a.btu, 10) - parseInt(b.btu, 10); 
                });

                // Если нет кондиционеров в заданном диапазоне, ищем ближайшее значение к upperBTU
                if (filteredConditioners.length === 0) {
                    const closestConditioners = data.filter(conditioner => parseInt(conditioner.btu, 10) > upperBTU);
                    if (closestConditioners.length > 0) {
                        // Сортируем по возрастанию и берем первую ближайшую границу
                        closestConditioners.sort((a, b) => parseInt(a.btu, 10) - parseInt(b.btu, 10));
                        const nextUpperBtu = closestConditioners[0].btu; // Берем ближайшее к upperBTU
                        filteredConditioners = closestConditioners.filter(conditioner => parseInt(conditioner.btu, 10) === parseInt(nextUpperBtu, 10));
                    } else {
                        noDataMessage.style.display = 'block'; 
                        noDataMessage.textContent = 'Кондиционеры не найдены в заданном диапазоне и выше.';
                    }
                }

                // Выводим отфильтрованные и отсортированные кондиционеры
                if (filteredConditioners.length > 0) {
                    filteredConditioners.forEach(conditioner => {
                        const row = document.createElement('tr');
                        row.innerHTML = `
                            <td><a href="${conditioner.url}" target="_blank">${conditioner.name}</a></td>
                            <td>${conditioner.price}</td>
                            <td>${conditioner.btu}</td>
                            <td>${conditioner.serviceArea} м²</td> 
                        `;
                        conditionerTableBody.appendChild(row); 
                    });
                } else {
                    noDataMessage.style.display = 'block'; 
                    noDataMessage.textContent = 'Ошибка в отображении данных.';
                }
            } else {
                noDataMessage.style.display = 'block'; 
                noDataMessage.textContent = 'Кондиционеры не найдены. Пожалуйста, попробуйте другой поиск.';
            }
        },
        error: function (xhr, status, error) {
            console.error('Ошибка:', error);
            console.error('Детали ошибки:', xhr.responseText); 

            const conditionerTableBody = document.querySelector('.conditioner-form tbody');
            conditionerTableBody.innerHTML = ''; 

            const noDataMessage = document.getElementById('noDataMessage');
            noDataMessage.style.display = 'block'; 
            noDataMessage.textContent = 'Произошла ошибка при получении кондиционеров. Пожалуйста, попробуйте позже.';
        }
    });
}






