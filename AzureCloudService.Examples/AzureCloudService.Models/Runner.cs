
namespace AzureCloudService.Utils
{
    using Infrastructures;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRunner
    {
        TResult Execute<TResult>(Func<TResult> func, string className = "", string funcName = "");
    }

    public class Runner : IRunner
    {
        public TResult Execute<TResult>(Func<TResult> func, string className = "", string funcName = "")
        {
            try
            {
                Debug.WriteLine($"{DateTime.UtcNow}: {className} {funcName} begin...");

                var result = func.Invoke();

                Debug.WriteLine($"{DateTime.UtcNow}: {className} {funcName} end.");

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTime.UtcNow}: {className} {funcName} error:{Environment.NewLine}{ex}");

                return default(TResult);
            }
        }
    }
}
