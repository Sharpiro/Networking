using Microsoft.VisualStudio.TestTools.UnitTesting;
using Networking.Models;
using System;
using System.Text;

namespace Networking.Tests
{
    [TestClass]
    public class CommandsTests
    {
        [TestMethod]
        public void CommandTest()
        {
            var socket = new FakeSocketChild();
            var socketMessage = new SocketMessage
            {
                MessageType = MessageType.Command,
                Data = Encoding.UTF8.GetBytes("TestCommand")
            };

            socket.TestCommandMethod(socketMessage);

            Assert.IsTrue(socket.CommandRan);
        }

        [TestMethod]
        public void BadCommandTest()
        {
            var socket = new FakeSocketChild();
            var socketMessage = new SocketMessage
            {
                MessageType = MessageType.Command,
                Data = Encoding.UTF8.GetBytes("TESTCommand")
            };

            Assert.ThrowsException<NullReferenceException>(() => socket.TestCommandMethod(socketMessage));
        }

        private class FakeSocketChild : BaseSocket
        {
            public bool CommandRan { get; set; }

            public void TestCommandMethod(SocketMessage socketMessage)
            {
                HandleCommand(socketMessage);
            }

            private void TestCommand(SocketMessage socketMessage)
            {
                CommandRan = true;
            }
        }
    }
}