using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.AddressSpace.Standard;
using OpcLabs.EasyOpc.UA.AlarmsAndConditions;
using OpcLabs.EasyOpc.UA.Extensions;
using OpcLabs.EasyOpc.UA.OperationModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpcLabsBug
{
	internal class Program
	{
		private static EasyUAClient client;

		[MTAThread]
		static void Main( string[] args )
		{
			client = new EasyUAClient();

			subscribe();
		}

		private static void subscribe()
		{
			UAEndpointDescriptor endpointDescriptor = new UAEndpointDescriptor( "opc.tcp://IIS3.adnet.local:53530/OPCUA/SimulationServer" );
			if ( client == null )
				client = new EasyUAClient();

			client.EventNotification += Client_EventNotification;
			UAAttributeFieldCollection uAAttributeFields = UABaseEventObject.AllFields;
			int eventMonitoredItem = client.SubscribeEvent(
				endpointDescriptor,
				UAObjectIds.Server,
				0,
				uAAttributeFields );
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
