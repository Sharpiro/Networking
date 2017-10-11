using Networking.Models;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public abstract class BaseSocket
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public event Action<string, string> CommandInvoked;
        public event Action<string, string> CommandReceived;

        protected void HandleCommand(SocketMessage socketMessage)
        {
            if (socketMessage == null) throw new ArgumentNullException(nameof(socketMessage));

            var commandName = Encoding.UTF8.GetString(socketMessage.Data);
            OnCommandReceived(socketMessage.ClientId, commandName);
            var thisType = GetType();
            var commandMethod = thisType.GetMethod(commandName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (commandMethod == null) commandMethod = thisType.BaseType.GetMethod(commandName, BindingFlags.NonPublic | BindingFlags.Instance);
            //if (commandMethod == null) throw new NullReferenceException($"Could not find command with name '{commandName}");
            if (commandMethod == null) return;
            commandMethod.Invoke(this, new[] { socketMessage });
            OnCommandInvoked(socketMessage.ClientId, commandName);
        }

        private async Task Test(SocketMessage socketMessage)
        {
            await Task.Yield();
        }

        protected void OnCommandInvoked(string clientId, string commandName) => CommandInvoked?.Invoke(clientId, commandName);
        protected void OnCommandReceived(string clientId, string commandName) => CommandReceived?.Invoke(clientId, commandName);
    }
}