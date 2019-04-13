using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zoro.Spider
{
    public class Avm2Asm
    {
        public static Op[] Trans(byte[] script)
        {
            ByteReader breader = new ByteReader(script);
            List<Op> arr = new List<Op>();
            while (breader.End == false)
            {
                Op o = new Op();
                o.addr = (UInt16)breader.addr;
                o.code = breader.ReadOP();
                try
                {
                    //push 特别处理
                    if (o.code >= OpCode.PUSHBYTES1 && o.code <= OpCode.PUSHBYTES75)
                    {
                        o.paramType = ParamType.ByteArray;
                        var count = (int)o.code;
                        o.paramData = breader.ReadBytes(count);
                    }
                    else
                    {
                        switch (o.code)
                        {
                            case OpCode.PUSH0:
                            case OpCode.PUSHM1:
                            case OpCode.PUSH1:
                            case OpCode.PUSH2:
                            case OpCode.PUSH3:
                            case OpCode.PUSH4:
                            case OpCode.PUSH5:
                            case OpCode.PUSH6:
                            case OpCode.PUSH7:
                            case OpCode.PUSH8:
                            case OpCode.PUSH9:
                            case OpCode.PUSH10:
                            case OpCode.PUSH11:
                            case OpCode.PUSH12:
                            case OpCode.PUSH13:
                            case OpCode.PUSH14:
                            case OpCode.PUSH15:
                            case OpCode.PUSH16:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.PUSHDATA1:
                                {
                                    o.paramType = ParamType.ByteArray;
                                    var count = breader.ReadByte();
                                    o.paramData = breader.ReadBytes(count);
                                }
                                break;
                            case OpCode.PUSHDATA2:
                                {
                                    o.paramType = ParamType.ByteArray;
                                    var count = breader.ReadUInt16();
                                    o.paramData = breader.ReadBytes(count);
                                }
                                break;
                            case OpCode.PUSHDATA4:
                                {
                                    o.paramType = ParamType.ByteArray;
                                    var count = breader.ReadInt32();
                                    o.paramData = breader.ReadBytes(count);
                                }
                                break;
                            case OpCode.NOP:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.JMP:
                            case OpCode.JMPIF:
                            case OpCode.JMPIFNOT:
                                o.paramType = ParamType.Addr;
                                o.paramData = breader.ReadBytes(2);
                                break;
                            case OpCode.SWITCH:
                                {
                                    o.paramType = ParamType.ByteArray;
                                    var count = breader.ReadInt16();
                                    o.paramData = BitConverter.GetBytes(count).Concat(breader.ReadBytes(count * 6)).ToArray();
                                }
                                break;
                            case OpCode.CALL:
                                o.paramType = ParamType.Addr;
                                o.paramData = breader.ReadBytes(2);
                                break;
                            case OpCode.RET:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.APPCALL:
                            case OpCode.TAILCALL:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(20);
                                break;
                            case OpCode.SYSCALL:
                                o.paramType = ParamType.String;
                                o.paramData = breader.ReadVarBytes(252);
                                break;
                            case OpCode.DUPFROMALTSTACK:
                            case OpCode.TOALTSTACK:
                            case OpCode.FROMALTSTACK:
                            case OpCode.XDROP:
                            case OpCode.XSWAP:
                            case OpCode.XTUCK:
                            case OpCode.DEPTH:
                            case OpCode.DROP:
                            case OpCode.DUP:
                            case OpCode.NIP:
                            case OpCode.OVER:
                            case OpCode.PICK:
                            case OpCode.ROLL:
                            case OpCode.ROT:
                            case OpCode.SWAP:
                            case OpCode.TUCK:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.CAT:
                            case OpCode.SUBSTR:
                            case OpCode.LEFT:
                            case OpCode.RIGHT:
                            case OpCode.SIZE:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.INVERT:
                            case OpCode.AND:
                            case OpCode.OR:
                            case OpCode.XOR:
                            case OpCode.EQUAL:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.INC:
                            case OpCode.DEC:
                            case OpCode.SIGN:
                            case OpCode.NEGATE:
                            case OpCode.ABS:
                            case OpCode.NOT:
                            case OpCode.NZ:
                            case OpCode.ADD:
                            case OpCode.SUB:
                            case OpCode.MUL:
                            case OpCode.DIV:
                            case OpCode.MOD:
                            case OpCode.SHL:
                            case OpCode.SHR:
                            case OpCode.BOOLAND:
                            case OpCode.BOOLOR:
                            case OpCode.NUMEQUAL:
                            case OpCode.NUMNOTEQUAL:
                            case OpCode.LT:
                            case OpCode.GT:
                            case OpCode.LTE:
                            case OpCode.GTE:
                            case OpCode.MIN:
                            case OpCode.MAX:
                            case OpCode.WITHIN:
                                o.paramType = ParamType.None;
                                break;

                            // Crypto
                            case OpCode.SHA1:
                            case OpCode.SHA256:
                            case OpCode.HASH160:
                            case OpCode.HASH256:
                            case OpCode.CSHARPSTRHASH32:
                            case OpCode.JAVAHASH32:
                            case OpCode.CHECKSIG:
                            case OpCode.CHECKMULTISIG:
                                o.paramType = ParamType.None;
                                break;

                            // Array
                            case OpCode.ARRAYSIZE:
                            case OpCode.PACK:
                            case OpCode.UNPACK:
                            case OpCode.PICKITEM:
                            case OpCode.SETITEM:
                            case OpCode.NEWARRAY:
                            case OpCode.NEWSTRUCT:
                                o.paramType = ParamType.None;
                                break;

                            // Exceptions
                            case OpCode.THROW:
                            case OpCode.THROWIFNOT:
                                o.paramType = ParamType.None;
                                break;

                            default:
                                throw new Exception("you fogot a type:" + o.code);
                        }
                    }
                }
                catch
                {
                    o.error = true;
                }
                arr.Add(o);
                if (o.error)
                    break;
            }
            return arr.ToArray();
        }
    }

    public class Op
    {
        public UInt16 addr;
        public bool error;
        public OpCode code;
        public byte[] paramData;
        public ParamType paramType;
        public override string ToString()
        {
            var name = getCodeName();
            if (paramType == ParamType.None)
            {

            }
            else if (paramType == ParamType.ByteArray)
            {
                name += "[" + AsHexString() + "]";
            }
            else if (paramType == ParamType.String)
            {
                name += "[" + AsString() + "]";
            }
            else if (paramType == ParamType.Addr)
            {
                name += "[" + AsAddr() + "]";
            }
            return addr.ToString("x04") + ":" + name;
        }
        public string AsHexString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("0x");
            foreach (var b in paramData)
            {
                sb.Append(b.ToString("x02"));
            }
            return sb.ToString();
        }
        public string AsString()
        {
            return System.Text.Encoding.ASCII.GetString(paramData);
        }
        public Int16 AsAddr()
        {
            return BitConverter.ToInt16(paramData, 0);
        }
        public string getCodeName()
        {
            var name = "";
            if (this.error)
                name = "[E]";
            if (this.code == OpCode.PUSHT)
                return "PUSH1(true)";
            if (this.code == OpCode.PUSHF)
                return "PUSH0(false)";
            if (code > OpCode.PUSHBYTES1 && code < OpCode.PUSHBYTES75)
                return name + "PUSHBYTES" + (code - OpCode.PUSHBYTES1 + 1);
            else
                return name + code.ToString();
        }
    }

    public enum ParamType
    {
        None,
        ByteArray,
        String,
        Addr,
    }
}
