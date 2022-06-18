using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using ChatWSServer;
using FluentAssertions;
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
            botManagerMock.Verify(m => m.SendMessage(It.Is<string>(it => it == chatMessage.RoomId),It.Is<string>(it => it == chatMessage.MessageContent)));
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

        [Test]
        public void TestGetRoomHistory_ShouldReturnOrderedMessages()
        {
            var roomId = "some-Id";
            var messageList = new List<ChatMessage>()
            {
                new ChatMessage() {Id = 1, RoomId = roomId, TimeStamp = DateTimeOffset.Now},
                new ChatMessage() {Id = 2, RoomId = roomId, TimeStamp = DateTimeOffset.MinValue},
                new ChatMessage() {Id = 3, RoomId = roomId, TimeStamp = DateTimeOffset.MaxValue}
            };

            var expectedOrder = new List<long>() {2, 1, 3};
            
            var botManagerMock = new Mock<IBotManager>();
            var repository = new Mock<IChatMessageRepository>();

            repository.Setup(r => r.GetMessages(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(messageList);
            
            var sut = new ChatManager(botManagerMock.Object, repository.Object);

            // Act
            var result = sut.GetRoomHistory("some-Id");

            result.Should().NotBeNull();
            result.Select(r => r.Id).Should().BeEquivalentTo(expectedOrder);
            repository.Verify(r => r.GetMessages(It.Is<string>(it => it == roomId), It.Is<int>(it => it == 50), It.Is<int>(it => it == 0)));
        }
        
    }
}