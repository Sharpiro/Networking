using Networking.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Networking
{
    public abstract class BaseSocket
    {
        public string Id { get; set; }

        public event Action<string> MessageLogged;
        public event Action<SocketMessage> CommandInvoked;
        public event Action<SocketMessage> CommandReceived;

        //todo: currently eating command exceptions
        protected void HandleCommand(SocketMessage socketMessage)
        {
            if (socketMessage == null) throw new ArgumentNullException(nameof(socketMessage));

            OnCommandReceived(socketMessage);
            var thisType = GetType();
            var commandMethod = thisType.GetMethod(socketMessage.Title, BindingFlags.NonPublic | BindingFlags.Instance);
            if (commandMethod == null) commandMethod = thisType.BaseType.GetMethod(socketMessage.Title, BindingFlags.NonPublic | BindingFlags.Instance);
            if (commandMethod == null) return;
            commandMethod.Invoke(this, new[] { socketMessage });
            OnCommandInvoked(socketMessage);
        }

        private async Task Test(SocketMessage socketMessage)
        {
            await Task.Yield();
        }

        protected void OnMessageLogged(string message)
        {
            //logger.LogInfo(message);
            MessageLogged?.Invoke(message);
        }
        protected void OnCommandInvoked(SocketMessage socketMessage) => CommandInvoked?.Invoke(socketMessage);
        protected void OnCommandReceived(SocketMessage socketMessage) => CommandReceived?.Invoke(socketMessage);
    }
}