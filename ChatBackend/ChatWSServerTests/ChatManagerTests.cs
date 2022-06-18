using AutoFixture;
using AutoFixture.AutoMoq;
using ChatWSServer;
using Moq;
using NUnit.Framework;

namespace ChatWSServerTests
{
    public class ChatManagerTests
    {
        private readonly Fixture Fixture;

        public ChatManagerTests()
        {
            Fixture = new Fixture();
            Fixture.Customize(new AutoMoqCustomization());
        }

        [Test]
        public void TestHandleMessage_ShouldSendMessageToBotQueueIfItsBotMessage()
        {
            // Arrange
            var botManagerMock = new Mock<IBotManager>();
            var repository = new Mock<IChatMessageRepository>();

            var chatMessage = Fixture.Build<ChatMessage>().Create();
            
            botManagerMock
                .Setup(m => m.IsBotCommand(It.IsAny<string>()))
                .Returns(true);

            var sut = new ChatManager(botManagerMock.Object, repository.Object);

            // Act
            sut.HandleMessage(chatMessage);

            // Assert
            botManagerMock.Verify(m => m.SendMessage(It.Is<string>(it => it == chatMessage.MessageContent)));
        }
        
        [Test]
        public void TestHandleMessage_ShouldSaveMessageIfItsNotBotMessage()
        {
            // Arrange
            var botManagerMock = new Mock<IBotManager>();
            var repository = new Mock<IChatMessageRepository>();

            var chatMessage = Fixture.Build<ChatMessage>().Create();
            
            botManagerMock
                .Setup(m => m.IsBotCommand(It.IsAny<string>()))
                .Returns(false);

            var sut = new ChatManager(botManagerMock.Object, repository.Object);

            // Act
            sut.HandleMessage(chatMessage);

            // Assert
            repository.Verify(r => r.AddAsync(It.IsAny<ChatMessage>()));
        }

    }
}