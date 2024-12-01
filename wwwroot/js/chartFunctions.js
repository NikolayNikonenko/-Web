let currentChart = null;

async function generateChart(chartElement, inputData, experimentData, chartTitle, yAxisLabel) {
    if (!chartElement || !(chartElement instanceof HTMLCanvasElement)) {
        console.error('chartElement не является элементом canvas');
        return;
    }

    // Удаление старого графика, если он существует
    if (currentChart) {
        currentChart.destroy();
    }

   const labels = Array.from(new Set([...inputData, ...experimentData].map(data => data.numberOfSrez)));

    const datasets = [];

    // Входные данные: измеренные значения
    datasets.push({
        label: 'Измеренные значения (входные)',
        data: labels.map(label => {
            const item = inputData.find(data => data.numberOfSrez === label);
            return item ? item.izmerValue : null;
        }),
        borderColor: 'blue',
        borderWidth: 2,
        fill: false,
        tension: 0.1
    });

    // Входные данные: оцененные значения, сгруппированные по Id1
    const inputGroupsById1 = groupBy(inputData, 'id1');
    for (const [id1, group] of Object.entries(inputGroupsById1)) {
        datasets.push({
            label: `Оцененные значения (входные) - Узел ${id1}`,
            data: labels.map(label => {
                const item = group.find(data => data.numberOfSrez === label);
                return item ? item.ocenValue : null;
            }),
            borderColor: randomColor(), // Генерируем случайный цвет для каждого Id1
            borderWidth: 2,
            fill: false,
            tension: 0.1
        });
    }

    // Экспериментальные данные
    if (experimentData && experimentData.length > 0) {
        // Экспериментальные данные: измеренные значения
        datasets.push({
            label: 'Экспериментальные измеренные',
            data: labels.map(label => {
                const filteredData = experimentData.filter(data => data.numberOfSrez === label);
                return filteredData.length > 0 ? filteredData[0].izmerValue : null;
            }),
            borderColor: 'orange',
            borderWidth: 1,
            borderDash: [5, 5], // Пунктирная линия
            fill: false,
            tension: 0.1
        });

        // Экспериментальные данные: оцененные значения, сгруппированные по Id1
        const experimentGroupsById1 = groupBy(experimentData, 'id1');
        for (const [id1, group] of Object.entries(experimentGroupsById1)) {
            datasets.push({
                label: `Экспериментальные оцененные - Узел ${id1}`,
                data: labels.map(label => {
                    const item = group.find(data => data.numberOfSrez === label);
                    return item ? item.ocenValue : null;
                }),
                borderColor: randomColor(), // Генерируем случайный цвет для каждого Id1
                borderWidth: 1,
                borderDash: [5, 5], // Пунктирная линия
                fill: false,
                tension: 0.1
            });
        }
    }

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
                }
            }
        }
    });
}
// Функция для группировки данных по ключу
function groupBy(array, key) {
    return array.reduce((result, item) => {
        const groupKey = item[key];
        if (!result[groupKey]) {
            result[groupKey] = [];
        }
        result[groupKey].push(item);
        return result;
    }, {});
}

// Функция для генерации случайного цвета
function randomColor() {
    return `#${Math.floor(Math.random() * 16777215).toString(16).padStart(6, '0')}`;
}
