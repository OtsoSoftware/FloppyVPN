﻿using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;

namespace FloppyVPN;

/// <summary>
/// 
/// </summary>
internal static class PaymentsManager
{
	public enum PaymentStatuses
	{
		waiting_deposit,
		confirming,
		not_enough,
		done, //when payment processed
		confirmed //when confirmed manually
	}

	public static void ConfirmPayment(string payment_id)
	{
		// Get payment data:
		DataRow payment = DB.GetDataTable("SELECT * FROM `payments` WHERE `id` = @payment_id;",
			new Dictionary<string, object>()
			{
				{ "@payment_id", payment_id }
			}).Rows[0];

		if ((bool)payment["is_paid"] == false)
		{
			Account acc = new(payment["login"].ToString());
			acc.AddTime((int)payment["months_to_add"]);

			DB.Execute("UPDATE `payments` SET `status` = @status, `is_paid` = @is_paid " +
				"WHERE `id` = @payment_id",
				new Dictionary<string, object>()
				{
					{ "@status", PaymentStatuses.confirmed.ToString() },
					{ "@is_paid", true },
					{ "@payment_id", payment_id }
				});
		}
	}

	public static string GenerateNewPaymentID()
	{
		generatingPaymentID:
		string new_payment_id = Cryption.GenerateRandomString(24);

		DataTable _paymentIds = DB.GetDataTable("SELECT * FROM `payments` WHERE `id` = @new_payment_id;",
			new Dictionary<string, object>()
			{
				{ "@new_payment_id", new_payment_id }
			});

		if (_paymentIds.Rows.Count > 0)
			goto generatingPaymentID;

		return new_payment_id;
	}

	/// <returns>Sum to pay for the specified months amount using specified currency</returns>
	private static decimal GetSum(string currency_code, int monthsAmount)
	{
		DataRow currencyInfo = DB.GetDataTable("SELECT * FROM `currencies` WHERE `currency_code` = @currency_code;",
			new Dictionary<string, object>()
			{
				{ "@currency_code", currency_code }
			}).Rows[0];

		if ((bool)currencyInfo["enabled"] == false)
			throw new Exception($"The desired currency '{currency_code}' seems to be disabled");

		decimal monthly_cost = (decimal)currencyInfo["month_cost"];

		decimal sum = monthly_cost * monthsAmount;

		return sum;
	}

	public static void WritePaymentIntoDB(string new_payment_id, string login, string currency_code, string network, string external_payment_id,
		int months_to_add, decimal amount_to_pay, string address_to_pay_to, string comment_to_attach, DateTime when_created, DateTime to_be_paid_till)
	{
		DB.Execute("INSERT INTO payments (id, login, network, currency_code, external_payment_id, months_to_add, amount_to_pay, address_to_pay_to, comment_to_attach, when_created, to_be_paid_till, status, is_paid) " +
		"VALUES(@id, @login, @network, @currency_code, @external_payment_id, @months_to_add, @amount_to_pay, @address_to_pay_to, @comment_to_attach, @when_created, @to_be_paid_till, @status, 0);",
		new Dictionary<string, object>()
		{
			{ "@id", new_payment_id },
			{ "@login", login },
			{ "@currency_code", currency_code },
			{ "@network", network },
			{ "@external_payment_id", external_payment_id },
			{ "@months_to_add", months_to_add },
			{ "@amount_to_pay", amount_to_pay },
			{ "@address_to_pay_to", address_to_pay_to },
			{ "@comment_to_attach", comment_to_attach },
			{ "@when_created", when_created },
			{ "@to_be_paid_till", to_be_paid_till },
			{ "@status", PaymentStatuses.waiting_deposit.ToString() }
		});

		Thread.Sleep(100);
	}

	/// <summary>
	/// Creates a new payment invoice depending on desired currency
	/// </summary>
	/// <returns>ID of the freshly created payment invoice</returns>
	public static string CreatePayment(string login, string currency_code, int months_to_add)
	{
		decimal sum = GetSum(currency_code, months_to_add);

		Account acc = new(login);
		if (!acc.exists)
			throw new Exception("The account we are trying to create a payment for does not seem to exist");
		else
			login = acc.login;

		// Determine which payment service this currency code belongs to:
		DataRow currency_data;
		try
		{
			currency_data = DB.GetDataTable(
				"SELECT * FROM `currencies` WHERE `currency_code` = @currency_code;",
				new Dictionary<string, object>()
				{
					{ "@currency_code", currency_code }
				}).Rows[0];
		}
		catch
		{
			throw new Exception("Requested currency not found");
		}

		string payment_service = currency_data["payment_service"].ToString();
		string currency_network = currency_data["network"].ToString();

		string new_payment_id;

		switch (payment_service)
		{
			case "nowpayments":
				new_payment_id = PaymentsServices.Create_NowPayments(login, sum, currency_code, months_to_add);
				break;
			case "p2p":
				new_payment_id = PaymentsServices.Create_P2P(login, sum, currency_code, currency_network, months_to_add);
				break;
			default:
				DB.Log("CreatePayment()", 
					$"Unable to find appropriate method to create an invoice " +
					$"using payment service {payment_service} for currency {currency_code}");
				throw new Exception("Approptiate invoice creation method not found");
		}

		return new_payment_id;
	}
}