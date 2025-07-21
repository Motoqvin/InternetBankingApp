using System.ComponentModel.DataAnnotations;

namespace InterBanking.Api.Models;

public class User
{
    public int Id { get; set; }
    public string ClientCode { get; set; }
    public string FullName { get; set; }
    public bool IsLocked { get; set; }
    public string Sid { get; set; }
    public string CellPhone { get; set; }
    public string EMail { get; set; }
    public string OtpMethod { get; set; }
    public string PinCode { get; set; }
    public string FinCode { get; set; }
    public string ClientType { get; set; }
    public string Otp { get; set; }
    public int Mistakes { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Status { get; set; }
    public string ExtendedClientCode { get; set; }
}