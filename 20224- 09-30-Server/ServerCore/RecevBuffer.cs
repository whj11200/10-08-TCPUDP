using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecevBuffer
    {
        //[] [] [] [r] [w] [] [] [] []
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;
        public RecevBuffer(int bufferSize)
        { 
          _buffer = new ArraySegment<byte>(new byte[bufferSize],0,bufferSize);
        }
        public int DataSize //클라에서 보낸 바이트수를 읽을 수 있는 유효범위
        {
            get { return _writePos - _readPos; }
        }
        public int FreeSize
        {
            get { return _buffer.Count - _writePos; }
        }
        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array,_buffer.Offset + _readPos,DataSize); }
        }
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array,_buffer.Offset +_writePos ,FreeSize); }
        }
        public void Clean()
        {
            int dataSize = DataSize;
            if(dataSize ==0)
            {
                 //남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                //남은 바이트가 있으면 시작위치로 복사 
                Array.Copy(_buffer.Array,_buffer.Offset+_readPos ,_buffer.Array,_buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }

        }
        public bool OnRead(int numOfBytes) //제대로 읽고 있는 확인 
        {
            if (numOfBytes > DataSize)
                return false;
            _readPos += numOfBytes;
            return true;
        }
        public bool OnWrite(int numOfBytes) //클라에서 제대로 신호가 byte가 왔는 확인 
        {
            if(numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;

            return true;
        }
    }
}
