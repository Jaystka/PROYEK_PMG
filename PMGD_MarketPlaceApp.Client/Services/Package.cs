using System;
using System.Text;

namespace PMGD_MarketPlaceApp.Client.Services {
    public class Package : IDisposable {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        public Package() {
            buffer = new List<byte>();
            readPos = 0;
        }

        public Package(int id) {
            buffer = new List<byte>();
            readPos = 0;

            Write(id);
        }

        public Package(byte[] data) {
            buffer = new List<byte>();
            readPos = 0;

            SetBytes(data);
        }

        #region Write Command
        public void Write(byte value) {
            buffer.Add(value);
        }

        public void Write(byte[] value) {
            buffer.AddRange(value);
        }

        public void Write(short value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(int value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(long value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(float value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(bool value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(string value) {
            Write(value.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(value));
        }
        #endregion

        #region Read Command
        public byte ReadByte(bool _moveReadPos = true) {
            if (buffer.Count > readPos) {
                byte _value = readableBuffer[readPos];
                if (_moveReadPos) {
                    readPos += 1;
                }
                return _value;
            } else {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        public byte[] ReadBytes(int _length, bool _moveReadPos = true) {
            if (buffer.Count > readPos) {
                byte[] _value = buffer.GetRange(readPos, _length).ToArray();
                if (_moveReadPos) {
                    readPos += _length;
                }
                return _value;
            } else {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        public short ReadShort(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                short value = BitConverter.ToInt16(readableBuffer, readPos); if (moveReadPos) {
                    readPos += 2;
                }
                return value;
            } else {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        public int ReadInt(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                int value = BitConverter.ToInt32(readableBuffer, readPos);
                if (moveReadPos) {
                    readPos += 4;
                }
                return value;
            } else {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        public long ReadLong(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                long value = BitConverter.ToInt64(readableBuffer, readPos);
                if (moveReadPos) {
                    readPos += 8;
                }
                return value;
            } else {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        public float ReadFloat(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                float _value = BitConverter.ToSingle(readableBuffer, readPos);
                if (moveReadPos) {
                    readPos += 4;
                }
                return _value;
            } else {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        public bool ReadBool(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                bool value = BitConverter.ToBoolean(readableBuffer, readPos);
                if (moveReadPos) {
                    readPos += 1;
                }
                return value;
            } else {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        public string ReadString(bool moveReadPos = true) {
            try {
                int length = ReadInt();
                string value = Encoding.ASCII.GetString(readableBuffer, readPos, length);
                if (moveReadPos && value.Length > 0) {
                    readPos += length;
                }
                return value;
            } catch {
                throw new Exception("Could not read value of type 'string'!");
            }
        }
        #endregion

        public void SetBytes(byte[] data) {
            Write(data);
            readableBuffer = buffer.ToArray();
        }

        public void WriteLength() {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }

        public void InsertInt(int _value) {
            buffer.InsertRange(0, BitConverter.GetBytes(_value));
        }

        public byte[] ToArray() {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public int Length() {
            return buffer.Count;
        }

        public int UnreadLength() {
            return Length() - readPos;
        }

        public void Reset(bool shouldReset = true) {
            if (shouldReset) {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            } else {
                readPos -= 4;
            }
        }

        #region Disposable Implement
        private bool disposed = false;

        protected virtual void Dispose(bool _disposing) {
            if (!disposed) {
                if (_disposing) {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
