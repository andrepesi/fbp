using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Transit
{
	public class Scheduler
	{
		public static void Main ()
		{
			new Scheduler ().Go ();
		}

		List<Component> components;
		List<Connection> connections;
		LinkedList<IEnumerator<object>> coroutines;

		public void Go ()
		{
			components = new List<Component> ();
			connections = new List<Connection> ();
			coroutines = new LinkedList<IEnumerator<object>> ();

			var reader = new FileReader ();
			reader.Name = "Get File Contents";
			components.Add (reader);

			var writer = new FileWriter ();
			writer.Name = "Write File Contents";
			components.Add (writer);

			foreach (var component in components) {
				foreach (var field in component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)) {
					foreach (Attribute attr in field.GetCustomAttributes(true)) {
						if (attr is InputPortAttribute) {
							var inputPort = attr as InputPortAttribute;
							CreatePort (component, field, inputPort.Name);
						} else if (attr is OutputPortAttribute) {
							var outputPort = attr as OutputPortAttribute;
							CreatePort (component, field, outputPort.Name);
						}
					}
				}
			}

			Connect (reader, "File Contents", writer, "Text To Write");
			SetInitialData (reader, "File Name", "test.txt");
			SetInitialData (writer, "File Name", "test2.txt");
		}

		void CreatePort (Component component, FieldInfo field, string name)
		{
			var createMethod = GetType ().GetMethod ("InstantiatePortForField", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod (field.FieldType);
			createMethod.Invoke (this, new object[] { component, field, name });
		}

		T InstantiatePortForField<T> (Component component, FieldInfo field, string name) where T : IPort
		{
			var port = Activator.CreateInstance<T> ();
			port.Name = name;
			field.SetValue (component, port);
			return port;
		}

		void Connect (Component firstComponent, string outPortName, Component secondComponent, string inPortName)
		{
			var outPort = GetOutPortFromComponentNamed (firstComponent, outPortName);
			var inPort = GetInPortFromComponentNamed (secondComponent, inPortName);

			Console.WriteLine ("found inPort: " + (inPort != null) + ", type: " + inPort.GetType ());
			if (inPort.HasConnection) {
				inPort.Connection.AddSender (outPort);
			} else {
				var connection = new Connection ();
				inPort.Connection = connection;
				outPort.Connection = connection;
				connection.SetReceiver (inPort);
				connection.AddSender (outPort);
			}
		}

		void SetInitialData (Component component, string portName, object value)
		{
			var ip = new InformationPacket (InformationPacket.PacketType.Content, value);
			GetInPortFromComponentNamed (component, portName).Connection.SetInitialData (ip);
		}

		IOutputPort GetOutPortFromComponentNamed (Component component, string name)
		{
			try {
				var matchingField = component.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance).First (field => {
					return field.GetCustomAttributes (true).FirstOrDefault (attr => {
						return (attr is OutputPortAttribute) && (attr as OutputPortAttribute).Name == name;
					}) != null;
				});

				return matchingField.GetValue (component) as IOutputPort;
			} catch (InvalidOperationException) {
				throw new InvalidOperationException (string.Format ("Component '{0}' of type '{1}' does not contain an output port named '{2}'", component.Name, component.GetType (), name));
			}
		}

		IInputPort GetInPortFromComponentNamed (Component component, string name)
		{
			try {
				var matchingField = component.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance).First (field => {
					return field.GetCustomAttributes (true).FirstOrDefault (attr => {
						return (attr is InputPortAttribute) && (attr as InputPortAttribute).Name == name;
					}) != null;
				});

				return matchingField.GetValue (component) as IInputPort;
			} catch (InvalidOperationException) {
				throw new InvalidOperationException (string.Format ("Component '{0}' of type '{1}' does not contain an input port named '{2}'", component.Name, component.GetType (), name));
			}
		}
	}
}