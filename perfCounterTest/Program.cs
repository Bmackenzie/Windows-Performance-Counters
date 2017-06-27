using System;
using System.Diagnostics;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Threading;

namespace perfCounterTest
{
    class Program
    {
        
        static void Main(string[] args)
        {
            PerformanceCounterMonitor perfCounter = new PerformanceCounterMonitor();
            perfCounter.GetCounterData();

            Console.Read();
        }
    }
}


class PerformanceCounterMonitor
{
    private PerformanceCounterCategory[] categories;
    List<PerformanceCounter> perfCounters;

    string[] catagoriesWanted = { "Processor", "System", "Cache", "LogicalDisk", "Memory", "PhysicalDisk", "Process" };
    string[] instancesWanted = {"_Total"};

    public PerformanceCounterMonitor()
    {
        categories = PerformanceCounterCategory.GetCategories();
        perfCounters = new List<PerformanceCounter>();

        foreach (PerformanceCounterCategory performanceCounterCategory in categories)
        {
            // Check to see if this is a catagory we would like to collect
            bool catagoryFound = Array.IndexOf(catagoriesWanted, performanceCounterCategory.CategoryName) > -1;
            if (catagoryFound)
            {
                if (PerformanceCounterCategory.Exists(performanceCounterCategory.CategoryName))
                {
                    string[] instances = performanceCounterCategory.GetInstanceNames();

                    foreach (string instance in instances)
                    {
                        bool instanceFound = Array.IndexOf(instancesWanted, instance) > -1;
                        if (instanceFound)
                        {
                            if (performanceCounterCategory.InstanceExists(instance))
                            {
                                PerformanceCounter[] counters = performanceCounterCategory.GetCounters(instance);
                                foreach (PerformanceCounter performanceCounter in counters)
                                {
                                    PerformanceCounter counter = new PerformanceCounter(performanceCounter.CategoryName, performanceCounter.CounterName, performanceCounter.InstanceName, performanceCounter.MachineName);
                                    counter.ReadOnly = true;
                                    counter.BeginInit();
                                    if (counter != null)
                                    {
                                        perfCounters.Add(counter);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void PrintCategories()
    {
        foreach (PerformanceCounterCategory performanceCounterCategory in categories)
        {
            if (PerformanceCounterCategory.Exists(performanceCounterCategory.CategoryName))
            {
                string[] instances = performanceCounterCategory.GetInstanceNames();
                if (instances.Length != 0)
                {
                    foreach (string instance in instances)
                    {
                        try
                        {
                            PerformanceCounter[] counters = performanceCounterCategory.GetCounters(instance);
                            foreach (PerformanceCounter performanceCounter in counters)
                            {
                                Console.WriteLine(performanceCounter.CategoryName);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
        }
    }

    public void GetCounterData()
    {
        Thread thread = new Thread(new ThreadStart(GetData));
        thread.Start();
    }

    private void GetData()
    {
        for (int i = 0; i < 30; i++)
        {
            foreach (PerformanceCounter counter in perfCounters)
            {
                float value = counter.NextValue();
                if (value > 0.0f)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;

                }
                Console.WriteLine("" + counter.CounterName + ": " + value.ToString());
            }
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Thread going to bed............................");
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Thread waking.........................");
        }
    }
}