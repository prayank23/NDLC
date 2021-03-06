﻿using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NDLC.Messages.JsonConverters
{
	public class ContractInfoJsonConverter : JsonConverter<ContractInfo>
	{
		class ContractData
		{
			[JsonProperty("sha256")]
			public string? SHA256 { get; set; }
			[JsonConverter(typeof(NBitcoin.JsonConverters.MoneyJsonConverter))]
			public Money? Sats { get; set; }
		}
		public override ContractInfo ReadJson(JsonReader reader, Type objectType, [AllowNull] ContractInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.StartObject)
				throw new JsonObjectException("ContractInfo is expecting a json object", reader);
			var cd = serializer.Deserialize<ContractData>(reader);
			if (cd?.Sats is null || cd?.SHA256 is null)
				throw new JsonObjectException("Invalid contract info (missing fields)", reader);

			DiscreteOutcome outcome = new DiscreteOutcome(Encoders.Hex.DecodeData(cd.SHA256));
			return new ContractInfo(outcome, cd.Sats);
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] ContractInfo value, JsonSerializer serializer)
		{
			if (value is ContractInfo)
			{
				var data = new ContractData();
				data.SHA256 = Encoders.Hex.EncodeData(value.Outcome.Hash);
				data.Sats = value.Payout;
				serializer.Serialize(writer, data);
			}
		}
	}
}
