using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using CSVExtension;

namespace CSVExtension.Tests
{
    [TestClass]
    public class CSVKernelExtensionTest
    {
        [TestMethod]
        public async Task Test_Vuoto()
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel()
            };

            new CSVKernelExtension().OnLoadAsync(kernel);
        }
    }
}
