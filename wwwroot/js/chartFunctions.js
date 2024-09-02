let currentChart = null;

function generateChart(chartElement, chartData, chartTitle, yAxisLabel) {
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
                                    `Коэффициент корреляции: ${data ? data.correlation : 'N/A'}`,
                                    `Статус ТМ: ${data ? data.status : 'N/A'}`
                                ];
                            }
                        }
                    }
                }
            }
        }
    });
}