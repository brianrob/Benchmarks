using System;
using System.Threading;
using System.Threading.Tasks;
using Ignitor;

namespace BenchmarksClient.Workers
{
    public partial class BlazorIgnitor
    {
        private Task Navigator(CancellationToken cancellationToken)
        {
            var links = new[] { "home", "fetchdata", "counter", "ticker" };
            var tasks = new Task[_clients.Count];
            for (var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];

                tasks[i] = Task.Run(async () =>
                {
                    var link = 0;
                    await client.RunAsync(cancellationToken);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await client.NavigateTo(links[link], cancellationToken);
                        await client.WaitUntil(hive => hive.TryFindElementById(links[link] + "_displayed", out _));
                        link = (link + 1) % links.Length;
                    }
                });
            }

            return Task.WhenAll(tasks);
        }

        private Task Clicker(CancellationToken cancellationToken)
        {
            var tasks = new Task[_clients.Count];
            for (var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];

                tasks[i] = Task.Run(async () =>
                {
                    await client.RunAsync(cancellationToken);
                    await client.NavigateTo("counter", cancellationToken);
                    ElementNode currentCount = null;
                    await client.WaitUntil(hive => hive.TryFindElementById("currentCount", out currentCount));

                    int currentValue;
                    var value = ReadIntAttribute(currentCount, "counter");
                    currentValue = value;

                    if (!client.ElementHive.TryFindElementById("counter", out var counter))
                    {
                        throw new Exception("counter button not found");
                    }

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await counter.ClickAsync(client.HubConnection, cancellationToken);
                        await client.WaitUntil(hive => ReadIntAttribute(currentCount, "counter") > currentValue);
                        currentValue = ReadIntAttribute(currentCount, "counter");
                    }
                });
            }

            return Task.WhenAll(tasks);
        }

        private Task BlazingPizza(CancellationToken cancellationToken)
        {
            var tasks = new Task[_clients.Count];
            for (var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];

                tasks[i] = Task.Run(async () =>
                {
                    await client.RunAsync(cancellationToken);
                    ElementNode pizza = null;
                    await client.WaitUntil(hive => hive.TryFindElementById("5", out pizza));

                    if (!client.ElementHive.TryFindElementById("pizzaOrders", out var pizzaOrders))
                    {
                        throw new InvalidOperationException("Can't find pizzaOrders");
                    }

                    var pizzaCount = 0;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await pizza.ClickAsync(client.HubConnection, cancellationToken);

                        ElementNode confirmButton = null;
                        await client.WaitUntil(hive => hive.TryFindElementById("Confirm", out confirmButton));
                        await confirmButton.ClickAsync(client.HubConnection, cancellationToken);

                        await client.WaitUntil(hive => ReadIntAttribute(pizzaOrders, "pizzaCount") > pizzaCount);
                        pizzaCount = ReadIntAttribute(pizzaOrders, "pizzaCount");
                    }
                });
            }

            return Task.WhenAll(tasks);
        }

        static int ReadIntAttribute(ElementNode currentCount, string attributeName)
        {
            if (!currentCount.Attributes.TryGetValue(attributeName, out var value))
            {
                throw new Exception($"{attributeName} attribute is missing");
            }

            return int.Parse(value.ToString());
        }
    }
}
