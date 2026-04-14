using System.Collections.Generic;
using System.Threading;

namespace POP_lab3
{
    class Program
    {
        private const int StorageSize = 5;
        private const int TotalItems = 20;
        private const int NumProducers = 3;
        private const int NumConsumers = 2;

        private Semaphore Access;
        private Semaphore Full;
        private Semaphore Empty;

        private readonly List<string> storage = new List<string>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(
                $"Storage: {StorageSize}  |  Total products: {TotalItems}" +
                $"  |  Producers: {NumProducers}  |  Consumers: {NumConsumers}\n");

            new Program().Starter(StorageSize, TotalItems, NumProducers, NumConsumers);
        }

        private void Starter(int storageSize, int totalItems, int numProducers, int numConsumers)
        {
            Access = new Semaphore(1, 1);
            Full = new Semaphore(storageSize, storageSize);
            Empty = new Semaphore(0, storageSize);

            int[] producerShares = Distribute(totalItems, numProducers);
            int[] consumerShares = Distribute(totalItems, numConsumers);

            var threads = new List<Thread>();

            for (int i = 0; i < numProducers; i++)
            {
                int producerId = i + 1;
                int producerItems = producerShares[i];
                Console.WriteLine($"[Producer {producerId,2}] Created. Will produce {producerItems} units of product.");

                Thread t = new Thread(() => Producer(producerId, producerItems))
                {
                    Name = $"Producer-{producerId}"
                };
                threads.Add(t);
            }

            Console.WriteLine();

            for (int i = 0; i < numConsumers; i++)
            {
                int consumerId = i + 1;
                int consumerItems = consumerShares[i];
                Console.WriteLine($"[Consumer {consumerId,2}] Created. Will consume {consumerItems} units of product.");

                Thread t = new Thread(() => Consumer(consumerId, consumerItems))
                {
                    Name = $"Consumer-{consumerId}"
                };
                threads.Add(t);
            }

            Console.WriteLine();

            foreach (Thread t in threads)
                t.Start();
        }

        private void Producer(int id, int itemCount)
        {
            Random rng = new Random();

            for (int i = 0; i < itemCount; i++)
            {
                Full.WaitOne();

                Access.WaitOne();

                string item = $"P{id}-item{i}";
                storage.Add(item);
                Console.WriteLine($"[Producer {id,2}] Added: {item,-14} | Storage: {storage.Count}");

                Access.Release();

                Empty.Release();

                // Thread.Sleep(rng.Next(100, 500));
            }

            Console.WriteLine($"[Producer {id,2}] Finished work.");
        }

        private void Consumer(int id, int itemCount)
        {
            Random rng = new Random();

            for (int i = 0; i < itemCount; i++)
            {
                Empty.WaitOne();

                Access.WaitOne();

                string item = storage[0];
                storage.RemoveAt(0);
                Console.WriteLine($"[Consumer {id,2}] Added: {item,-14} | Storage: {storage.Count}");

                Access.Release();

                Full.Release();

                // Thread.Sleep(rng.Next(100, 500));
            }

            Console.WriteLine($"[Consumer {id,2}] Finished work.");
        }

        private static int[] Distribute(int total, int parts)
        {
            int[] shares = new int[parts];
            int baseVal = total / parts;
            int remainder = total % parts;

            for (int i = 0; i < parts; i++)
                shares[i] = baseVal + (i < remainder ? 1 : 0);

            return shares;
        }
    }
}
