
let chartInstances = new Map();

function drawDonutChart(chartElement, successfulCount, totalCount) {
    console.log('Вызов функции drawDonutChart с параметрами: successfulCount =', successfulCount, ', totalCount =', totalCount);

    if (!chartElement) {
        console.error('Элемент canvas не найден.');
        return;
    }

    const ctx = chartElement.getContext('2d');
    if (!ctx) {
        console.error('Контекст canvas не найден.');
        return;
    }

    // Уничтожить предыдущий график, если он уже существует
    if (chartInstances.has(chartElement)) {
        chartInstances.get(chartElement).destroy();
    }

    // Создать новую диаграмму и сохранить её
    const newChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Успешные', 'Неуспешные'],
            datasets: [{
                data: [successfulCount, totalCount - successfulCount],
                backgroundColor: ['#4caf50', '#f44336'],
                hoverBackgroundColor: ['#66bb6a', '#e57373']
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });

    chartInstances.set(chartElement, newChartInstance);
}