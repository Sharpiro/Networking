using Networking.Models;
using System;
using System.Reflection;
using System.Text;

namespace Networking
{
    public abstract class BaseSocket
    {
        protected void HandleCommand(SocketMessage socketMessage)
        {
            //var commandNumber = socketMessage.Data[0];
            //var command = (Command)commandNumber;
            //var commandText = command.ToString();
            var commandText = Encoding.UTF8.GetString(socketMessage.Data);

            var thisType = GetType();

            var commandMethod = thisType.GetMethod(commandText, BindingFlags.NonPublic | BindingFlags.Instance);
            if (commandMethod == null) commandMethod = thisType.BaseType.GetMethod(commandText, BindingFlags.NonPublic | BindingFlags.Instance);
            if (commandMethod == null) throw new NullReferenceException($"Could not find command with name '{commandText}");
            commandMethod.Invoke(this, new[] { socketMessage });

            //switch (command)
            //{
            //    case Command.Test:
            //        Test(socketMessage);
            //        break;
            //    default: throw new ArgumentOutOfRangeException();
            //}
        }

        //private void Test(SocketMessage socketMessage)
        //{

        //}
    }

    //public enum Command
    //{
    //    TestCommand = 0
    //}
}