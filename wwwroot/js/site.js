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

    // Установка фиксированных размеров для канваса через JS (если необходимо)
    //chartElement.width = chartElement.parentElement.offsetWidth; // Ширина зависит от родительского контейнера
    //chartElement.height = chartElement.parentElement.offsetHeight; // Высота зависит от родительского контейнера

    chartElement.width = 280; // Ширина зависит от родительского контейнера
    chartElement.height = 280; // Высота зависит от родительского контейнера


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
            responsive: false,
            maintainAspectRatio: true,
            layout: {
                padding: {
                    top: 20, // Добавьте отступ сверху
                    bottom: 20, // Добавьте отступ снизу
                    left: 20, // Отступ слева
                    right: 20 // Отступ справа
                }
            },
            plugins: {
                legend: {
                    position: 'top', // Позиция легенды
                    align: 'center' // Выравнивание легенды по центру
                }
            }
        }
    });

    chartInstances.set(chartElement, newChartInstance);
}