﻿using NBitcoin.DataEncoders;
using NBitcoin.Secp256k1;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading.Tasks;

namespace NDLC.CLI
{
	public class AddSetOracleCommand : CommandBase
	{
		public static Command CreateCommand(bool set)
		{
			if (set)
			{
				Command command = new Command("set", "Modify an oracle")
				{
					new Argument<string>("name", "The oracle name"),
					new Argument<string>("pubkey", "The oracle pubkey"),
				};
				command.Handler = new AddSetOracleCommand() { Set = true };
				return command;
			}
			else
			{
				Command command = new Command("add", "Add a new oracle")
				{
					new Argument<string>("name", "The oracle name"),
					new Argument<string>("pubkey", "The oracle pubkey"),
				};
				command.Handler = new AddSetOracleCommand();
				return command;
			}
		}
		public bool Set { get; set; }
		protected override async Task InvokeAsyncBase(InvocationContext context)
		{
			var oracleName = context.ParseResult.CommandResult.GetArgumentValueOrDefault<string>("name")?.Trim();
			if (oracleName is null)
				throw new CommandOptionRequiredException("name");
			var pubkey = context.ParseResult.CommandResult.GetArgumentValueOrDefault<string>("pubkey")?.ToLowerInvariant()?.Trim();
			if (pubkey is null)
				throw new CommandOptionRequiredException("pubkey");
			var exists = await Repository.OracleExists(oracleName);

			if (exists && !Set)
				throw new CommandException("name", "This oracle already exists");
			if (!exists && Set)
				throw new CommandException("name", "This oracle does not exists");

			ECXOnlyPubKey? pubkeyObj;
			try
			{
				if (!ECXOnlyPubKey.TryCreate(Encoders.Hex.DecodeData(pubkey), Context.Instance, out pubkeyObj) || pubkeyObj is null)
					throw new CommandException("pubkey", "Invalid pubkey");
			}
			catch (FormatException)
			{
				throw new CommandException("pubkey", "Invalid pubkey");
			}
			await Repository.SetOracle(oracleName, pubkeyObj);
		}
	}
}
