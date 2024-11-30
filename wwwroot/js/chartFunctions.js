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

    const labels = Array.from(new Set(inputData.concat(experimentData).map(data => data.numberOfSrez)));

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

    // Входные данные: оцененные значения
    datasets.push({
        label: 'Оцененные значения (входные)',
        data: labels.map(label => {
            const item = inputData.find(data => data.numberOfSrez === label);
            return item ? item.ocenValue : null;
        }),
        borderColor: 'green',
        borderWidth: 2,
        fill: false,
        tension: 0.1
    });

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

    // Экспериментальные данные: оцененные значения
    datasets.push({
        label: 'Экспериментальные оцененные',
        data: labels.map(label => {
            const filteredData = experimentData.filter(data => data.numberOfSrez === label);
            return filteredData.length > 0 ? filteredData[0].ocenValue : null;
        }),
        borderColor: 'red',
        borderWidth: 1,
        borderDash: [5, 5], // Пунктирная линия
        fill: false,
        tension: 0.1
    });

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
