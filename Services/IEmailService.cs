using ControlDeGastosMVC.API.Models;
namespace ControlDeGastosMVC.API.Services
{
    public interface IEmailService
    {
        void SendEmail(EmailDTO request);
    }
}
