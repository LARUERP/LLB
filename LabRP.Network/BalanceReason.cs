namespace LabRP.Network;

public enum BalanceReason : byte
{
	Initial,
	Paycheck,
	PaymentSent,
	PaymentReceived,
	Admin,
	Denied
}
