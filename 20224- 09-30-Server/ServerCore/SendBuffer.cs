﻿namespace ServerCore
{

    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = 
            new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096 *100;
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);

        }
        public static ArraySegment<byte> Close(int usedSize) => CurrentBuffer.Value.Close(usedSize);
    }
    public class SendBuffer
    {
        //[u][][][][][][][]
        byte[] _buffer;
        int _usedSize = 0;
        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }
        public int FreeSize 
        {
            get { return _buffer.Length - _usedSize; }
        }
        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return null;
            return new ArraySegment<byte>(_buffer, _usedSize,reserveSize);
        }
        public ArraySegment<byte>Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }

    }
}
