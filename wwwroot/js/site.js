
let chartInstance = null;

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

    // Уничтожить предыдущий график, если он существует
    if (chartInstance) {
        chartInstance.destroy();
    }

    chartInstance = new Chart(ctx, {
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
}