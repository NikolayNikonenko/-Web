let currentChart = null;

function generateChart(chartElement, chartData, tmData, chartTitle, yAxisLabel) {
    if (!chartElement || !(chartElement instanceof HTMLCanvasElement)) {
        console.error('chartElement не является элементом canvas');
        return;
    }

    // Удаление старого графика, если он существует
    if (currentChart) {
        currentChart.destroy();
    }

    // Группировка данных по значению id1
    const groupedData = chartData.reduce((acc, data) => {
        if (!acc[data.id1]) {
            acc[data.id1] = [];
        }
        acc[data.id1].push(data);
        return acc;
    }, {});

    // Подготовка данных для графика
    const labels = Array.from(new Set(chartData.map(data => data.numberOfSrez))); // Уникальные метки по оси X - наименования срезов
    const datasets = Object.keys(groupedData).map(id1 => {
        const group = groupedData[id1];
        const ocenValues = new Array(labels.length).fill(undefined);

        group.forEach(data => {
            const index = labels.indexOf(data.numberOfSrez);
            if (index !== -1) {
                ocenValues[index] = data.ocenValue;
            }
        });

        return {
            label: `Оцененные значения (Узел: ${id1})`,
            data: ocenValues,
            borderColor: `rgba(${Math.floor(Math.random() * 255)}, ${Math.floor(Math.random() * 255)}, ${Math.floor(Math.random() * 255)}, 1)`,
            borderWidth: 1,
            fill: false,
            tension: 0.1
        };
    });

    // Добавление измеренных значений в качестве отдельного набора данных
    const izmerValues = new Array(labels.length).fill(undefined);
    chartData.forEach(data => {
        const index = labels.indexOf(data.numberOfSrez);
        if (index !== -1 && !izmerValues[index]) {
            izmerValues[index] = data.izmerValue;
        }
    });

    datasets.unshift({
        label: 'Измеренные значения',
        data: izmerValues,
        borderColor: 'blue',
        borderWidth: 1,
        fill: false,
        tension: 0.1
    });

    // Получение данных о корреляции и статусе из таблицы tm
    const tmData = await getTmData(chartData.map(data => data.indexTM));

    // Создание нового графика
    const ctx = chartElement.getContext('2d');
    currentChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Срезы'
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: yAxisLabel
                    }
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: chartTitle
                },
                tooltip: {
                    callbacks: {
                        label: function (tooltipItem) {
                            const datasetLabel = tooltipItem.dataset.label;
                            const data = chartData.find(d => d.numberOfSrez === tooltipItem.label && (
                                datasetLabel.includes("Измеренные значения") ? d.izmerValue === tooltipItem.raw : d.ocenValue === tooltipItem.raw
                            ));

                            // Получение коэффициента корреляции и статуса из tmData
                            const tmInfo = tmData.find(tm => tm.indexTM === data.indexTM);
                            const correlation = tmInfo ? tmInfo.correlation : 'N/A';
                            const status = tmInfo ? tmInfo.status : 'N/A';
                            const id1 = data ? data.id1 : 'N/A';

                            if (datasetLabel.includes("Измеренные значения")) {
                                return [
                                    `Узел: ${id1}`,
                                    `Тип: Измеренное`,
                                    `Значение: ${tooltipItem.raw}`,
                                ];
                            } else {
                                return [
                                    `Узел: ${id1}`,
                                    `Тип: Оцененное`,
                                    `Значение: ${tooltipItem.raw}`,
                                    `Коэффициент корреляции: ${correlation}`,
                                    `Статус ТМ: ${status}`
                                ];
                            }
                        }
                    }
                }
            }
        }
    });
}
// Функция для получения данных из таблицы tm
async function getTmData(indexTMs) {
    // Замените этот код на ваш API-запрос или запрос к базе данных
    const response = await fetch(`/api/tm?indexTMs=${indexTMs.join(',')}`);
    if (!response.ok) {
        console.error('Ошибка при получении данных из таблицы tm');
        return [];
    }
    return await response.json(); // Предполагается, что данные приходят в формате JSON
}