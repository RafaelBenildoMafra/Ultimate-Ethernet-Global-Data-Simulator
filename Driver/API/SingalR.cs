using Microsoft.AspNetCore.SignalR;

namespace EthernetGlobalData.API
{
    public sealed class SingalR : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("RecieveData", $"{Context.ConnectionId} has joined");
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("RecieveMessage", $"{Context.ConnectionId}: {message}");
        }
    }
}
