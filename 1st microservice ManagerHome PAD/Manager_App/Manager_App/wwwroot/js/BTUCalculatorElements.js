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

function clearForm() {
    const form = document.getElementById("btuCalculatorForm");
    form.reset();

    // Set all input values to '0'
    const inputs = form.querySelectorAll("input[type='text'], input[type='number']");
    inputs.forEach(input => {
        input.value = '0';
    });

    // Hide validation errors
    const validationErrors = document.querySelector(".validation-summary-errors");
    if (validationErrors) {
        validationErrors.style.display = "none";
    }

    // Hide calculation results
    const calculationResults = document.getElementById('calculationResults');
    calculationResults.style.display = "none";

    // Hide additional parameters
    const additionalParams = document.getElementById('additionalParams');
    additionalParams.style.display = 'none';
}

function calculateBTU() {
    const formData = new FormData(document.getElementById('btuCalculatorForm'));
    console.log(formData);
    const jsonData = {};

    // Преобразуем FormData в JSON
    formData.forEach((value, key) => {
        let newKey = key.replace("RequestModel.", "");
        let newValue;
        console.log(key, ":", value);
        if (newKey == "PeopleCount" || newKey == "NumberOfComputers" || newKey == "NumberOfTVs" || newKey == "RoomSize" || newKey == "CeilingHeight" || newKey == "OtherAppliancesKWattage") {
            newValue = Number(value);
        }
        else if (newKey == "HasVentilation" || newKey == "Guaranteed20Degrees" || newKey == "IsTopFloor" || newKey == "HasLargeWindow") {
            newValue = Boolean(value);
        }
        else if (newKey == "AirExchangeRate" || newKey == "WindowArea") {
            try {
                newValue = Number(value);
            }
            catch (error) {
                newValue = value;
                console.error("An error occurred:", error);
            }
        }
        else {
            newValue = value;
        }

        jsonData[newKey] = newValue;
    });
    console.log("Отправляемые данные:", jsonData);

    // Проверка обязательных полей
    if (!jsonData['SizeUnit'] || !jsonData['HeightUnit'] || !jsonData['SunExposure']) {
        console.error("Обязательные поля не заполнены:", jsonData);
        alert("Пожалуйста, заполните все обязательные поля."); // Предупреждение пользователю
        return; // Прерываем выполнение функции
    }

    // Логируем данные перед отправкой
    console.log("Отправляемые данные:", jsonData);

    $.ajax({
        url: '/CalculateBTU/CalculateBtuValue', // Укажите правильный URL вашего микросервиса
        type: 'POST',
        data: JSON.stringify(jsonData), // Отправляем данные как JSON
        contentType: 'application/json', // Указываем тип содержимого
        success: function (data) {
            const resultsContainer = document.getElementById('calculationResults');
            resultsContainer.innerHTML = `
                <div class="btu-result-item-btu">
                    <span class="btu-label-btu">Calculated Power (BTU):</span>
                    <span class="btu-value-btu">${data.CalculatedPowerBTU}</span>
                </div>
                <div class="btu-result-item-btu">
                    <span class="btu-label-btu">Recommended Range (BTU):</span>
                    <span class="btu-value-btu">${data.RecommendedRangeBTU.Lower} - ${data.RecommendedRangeBTU.Upper}</span>
                </div>
                <div class="btu-result-item-kw">
                    <span class="btu-label-kw">Calculated Power (kW):</span>
                    <span class="btu-value-kw">${data.CalculatedPowerKW}</span>
                </div>
                <div class="btu-result-item-kw">
                    <span class="btu-label-kw">Recommended Range (kW):</span>
                    <span class="btu-value-kw">${data.RecommendedRangeKW.Lower} - ${data.RecommendedRangeKW.Upper}</span>
                </div>
            `;
            resultsContainer.style.display = 'block'; // Показываем результаты
        },
        error: function (xhr, status, error) {
            console.error('Ошибка AJAX:', xhr.responseText); // Логируем текст ошибки
        }
    });
}


/*function updateConditioners() {
    $.ajax({
        url: '/CalculateBTU/UpdateConditioners',
        type: 'POST',
        success: function (data) {
            const conditionerTableBody = document.querySelector('.conditioner-form tbody');
            conditionerTableBody.innerHTML = ''; 

            data.forEach(conditioner => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td><a href="${conditioner.Url}" target="_blank">${conditioner.Name}</a></td>
                    <td>${conditioner.Price}</td>
                    <td>${conditioner.BTU}</td>
                    <td>${conditioner.ServiceArea}</td>
                `;
                conditionerTableBody.appendChild(row);
            });

            if (data.length === 0) {
                const noDataMessage = document.createElement('p');
                noDataMessage.style.color = 'red';
                noDataMessage.textContent = 'No conditioners found. Please try another search.';
                conditionerTableBody.appendChild(noDataMessage);
            }
        },
        error: function (error) {
            console.error('Error:', error);
        }
    });
}
*/