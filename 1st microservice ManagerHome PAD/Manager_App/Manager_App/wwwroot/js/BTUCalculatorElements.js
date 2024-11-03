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

function toggleWindowArea() {
    const hasLargeWindow = document.getElementById('hasLargeWindow').value;
    const windowAreaGroup = document.getElementById('windowAreaGroup');
    windowAreaGroup.style.display = (hasLargeWindow === 'true') ? 'block' : 'none';
}

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
}


function calculateBTU() {
    const formData = new FormData(document.getElementById('btuCalculatorForm'));
    const jsonData = {};

    formData.forEach((value, key) => {
        let newKey = key.replace("RequestModel.", "");
        let newValue;
        console.log(key, ":", value);
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
    //console.log("Отправляемые данные:", jsonData);

    if (!jsonData['SizeUnit'] || !jsonData['HeightUnit'] || !jsonData['SunExposure']) {
        console.error("Обязательные поля не заполнены:", jsonData);
        alert("Пожалуйста, заполните все обязательные поля."); 
        return; 
    }

    // Логируем данные перед отправкой
    console.log("Отправляемые данные:", jsonData);

    $.ajax({
        url: '/CalculateBTU/CalculateBtuValue', 
        type: 'POST',
        data: JSON.stringify(jsonData), 
        contentType: 'application/json', 
        success: function (data) {
            console.log("Данные, полученные от сервера:", data); 
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
            conditionerTableBody.innerHTML = ''; // Очищаем таблицу

            const noDataMessage = document.getElementById('noDataMessage');
            if (data && data.length > 0) { // Проверка на наличие данных
                noDataMessage.style.display = 'none'; // Скрываем сообщение о том, что нет данных
                data.forEach(conditioner => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td><a href="${conditioner.URL}" target="_blank">${conditioner.NAME}</a></td>
                        <td>${conditioner.PRICE}</td>
                        <td>${conditioner.BTU}</td>
                        <td>${conditioner['SERVICE AREA']}</td>
                    `;
                    conditionerTableBody.appendChild(row); // Добавляем строку в таблицу
                });
            } else {
                noDataMessage.style.display = 'block'; // Показываем сообщение о том, что нет данных
                noDataMessage.textContent = 'Кондиционеры не найдены. Пожалуйста, попробуйте другой поиск.';
            }
        },
        error: function (xhr, status, error) {
            console.error('Ошибка:', error);
            console.error('Детали ошибки:', xhr.responseText); // Логируем текст ответа от сервера

            const conditionerTableBody = document.querySelector('.conditioner-form tbody');
            conditionerTableBody.innerHTML = ''; // Очищаем таблицу

            const noDataMessage = document.getElementById('noDataMessage');
            noDataMessage.style.display = 'block'; // Показываем сообщение об ошибке
            noDataMessage.textContent = 'Произошла ошибка при получении кондиционеров. Пожалуйста, попробуйте позже.';
        }
    });
}





