using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.AddressSpace.Standard;
using OpcLabs.EasyOpc.UA.AlarmsAndConditions;
using OpcLabs.EasyOpc.UA.Extensions;
using OpcLabs.EasyOpc.UA.OperationModel;
using System;
using System.Collections.Generic;

namespace OpcLabsBug
{
	internal class Program
	{
		private static EasyUAClient client;

		static void Main( string[] args )
		{
			UAEndpointDescriptor endpointDescriptor = new UAEndpointDescriptor( "opc.tcp://127.0.0.1" ).WithUserNameIdentity( "user", "pass" );
			client = new EasyUAClient();

			client.EventNotification += Client_EventNotification;
			UAAttributeFieldCollection uAAttributeFields = UABaseEventObject.AllFields;
			int eventMonitoredItem = client.SubscribeEvent(
				endpointDescriptor,
				UAObjectIds.Server,
				0,
				uAAttributeFields );

			List<string> recordIds = new List<string>
			{
				"ns=3;s=\"DB_INTS_RO\".int5",
				"ns=3;s=\"DB_INTS_RO\".int6",
				"ns=3;s=\"DB_INTS_RO\".int7",
				"ns=3;s=\"DB_INTS_RO\".int8",
				"ns=3;s=\"DB_INTS_RO\".dint5",
				"ns=3;s=\"DB_INTS_RO\".dint6",
				"ns=3;s=\"DB_INTS_RO\".dint7",
				"ns=3;s=\"DB_INTS_RO\".dint8",
			};

			foreach ( string recordId in recordIds )
			{
				if ( !string.IsNullOrEmpty( recordId ) )
					client.SubscribeDataChange( endpointDescriptor, recordId, 0, ProcessDataChange );
			}


			while ( Console.ReadLine() != "end" )
			{
				Console.Clear();
			}
		}

		private static void ProcessDataChange( object sender, EasyUADataChangeNotificationEventArgs eventArgs )
		{
			if ( eventArgs.Succeeded )
			{
				string recordId = eventArgs.Arguments.NodeDescriptor.NodeId.ExpandedText;
				var value = eventArgs.AttributeData.Value;
				var time = eventArgs.AttributeData.SourceTimestampLocal;

				Console.WriteLine( $"Value change: {recordId} - {value} - {time}" );
			}
		}

		private static void Client_EventNotification( object sender, EasyUAEventNotificationEventArgs e )
		{
			if ( e.EventData == null )
				return;

			var Time = Convert.ToDateTime( e.EventData.FieldResults[UABaseEventObject.Operands.Time].Value ).ToLocalTime();
			var EventText = e.EventData.FieldResults[UABaseEventObject.Operands.Message].Value.ToString();
			Console.WriteLine( $"Event: {EventText}  - {Time}" );
		}
	}
}
