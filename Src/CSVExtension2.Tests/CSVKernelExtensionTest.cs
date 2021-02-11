using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using System.Threading.Tasks;
using CSVExtension;
using NUnit.Framework;

namespace CSVExtension2.Tests
{
    public class CSVKernelExtensionTest
    {
        [Test]
        public async Task Test_Vuoto()
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel()
            };

            await new CSVKernelExtension().OnLoadAsync(kernel);
        }
    }
}
