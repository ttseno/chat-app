using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using StocksBot;
using StocksBot.Configuration;

namespace StocksBotTests
{
    public class StooqClientTest
    {
        private readonly Fixture Fixture;

        public StooqClientTest()
        {
            Fixture = new Fixture();
            Fixture.Customize(new AutoMoqCustomization());
        }

        [Test]
        public async Task TestGetStockQuotation_ShouldReturnExpectedValue()
        {
            // Arrange
            var fileString = @"Symbol,Date,Time,Open,High,Low,Close,Volume
AAPL.US,2022-06-17,22:00:08,130.065,133.079,129.81,131.56,134111934
";
            var httpClientMock = new Mock<HttpMessageHandler>();
            var AppConfiguration = Fixture.Build<AppConfiguration>()
                .With(c => c.StooqApiBaseUrl, "http://localhost")
                .Create();

            // It is necessary to setup the mock like this because the HttpClient is a concrete class
            // and can't be mocked, so we have to mock the base abstract class
            httpClientMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fileString)
                })
                .Verifiable();
            

            var sut = new StooqClient(new HttpClient(httpClientMock.Object), AppConfiguration);

            var expectedModel = new StooqModel()
            {
                Symbol = "AAPL.US",
                Date = "2022-06-17",
                Time = "22:00:08",
                Open = 130.065,
                High = 133.079,
                Low = 129.81,
                Close = 131.56,
                Volume = 134111934
            };

            var stock = "AAPL.US";
            
            // Act
            var result = await sut.GetStockQuotation(stock);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedModel);
        }
    }
}