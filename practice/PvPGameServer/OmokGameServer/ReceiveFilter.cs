﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace PvPGameServer
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        public string SessionID;
        public byte[] Data;

        public const int PACKET_HEADER_MSGPACK_START_POS = 3;

        // UInt16 TotalSize, UInt16 PacketID, Byte Type
        public const int HEADERE_SIZE = 5 + PACKET_HEADER_MSGPACK_START_POS;
                
        public EFBinaryRequestInfo(byte[] packetData) : base(null, packetData)
        {
            Data = packetData;
        }

    }

    public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(EFBinaryRequestInfo.HEADERE_SIZE)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, offset, 2);

            var totalSize = BitConverter.ToUInt16(header, offset + EFBinaryRequestInfo.PACKET_HEADER_MSGPACK_START_POS);
            return totalSize - EFBinaryRequestInfo.HEADERE_SIZE;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(header.Array, 0, EFBinaryRequestInfo.HEADERE_SIZE);
            }
				
            // default로 ResolveRequestInfo는 Body 데이터만 반환한다.
            // 그러나 우리는 헤더와 body를 같이 MsgPack으로 인코딩 했기 때문에 Header를 포함한 데이터를 반환해야 한다.
			if (bodyBuffer != null)
            {
                var packetStartPos = offset - EFBinaryRequestInfo.HEADERE_SIZE;
				var packetSize = length + EFBinaryRequestInfo.HEADERE_SIZE;
				return new EFBinaryRequestInfo(bodyBuffer.CloneRange(packetStartPos, packetSize));          
            }
            else
            {
                return new EFBinaryRequestInfo(header.Array);
            }			
        }
    }
}
