using System;
using NTransit;
using System.Collections.Generic;

namespace NTransitTest {
	public class MockOutputPort : IOutputPort {
		public string Name { get; set; }
		public Component Process { get; set; }
		public bool HasCapacity { get; set; }
		public bool Connected { get; set; }
		public bool Closed { get; set; }
		public bool ConnectedPortClosed { get; set; }
		public bool ResponseToUseForTrySend { get; set; }

		public List<InformationPacket> SentPackets { get; private set; }

		public MockOutputPort() : this(null) { }

		public MockOutputPort(Component process) { 
			HasCapacity = true;
			Connected = true;
			Closed = false;
			ConnectedPortClosed = false;
			ResponseToUseForTrySend = true;
			SentPackets = new List<InformationPacket>();
		}

		public void ConnectTo(IInputPort port) {
			throw new NotImplementedException();
		}

		public bool TrySend(InformationPacket ip) {
			SentPackets.Add(ip);
			return ResponseToUseForTrySend;
		}

		public bool TrySend(InformationPacket ip, bool ignoreClosed) {
			SentPackets.Add(ip);
			return ResponseToUseForTrySend;
		}

		public void Close() {
			Closed = true;
		}
	}
}