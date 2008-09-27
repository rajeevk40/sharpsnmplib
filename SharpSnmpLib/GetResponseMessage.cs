/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2008/5/1
 * Time: 18:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Lextm.SharpSnmpLib
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    
    /// <summary>
    /// GET response message.
    /// </summary>
    public class GetResponseMessage : ISnmpMessage
    {
        private ISnmpPdu _pdu;
        private int _sequenceNumber;
        private ErrorCode _errorStatus;
        private int _errorIndex;
        private IList<Variable> _variables;
        private byte[] _bytes;
        private VersionCode _version;
        private OctetString _community;
        private IPAddress _receiver;
        
        /// <summary>
        /// Creates a <see cref="GetResponseMessage"/> with all contents.
        /// </summary>
        /// <param name="version">Protocol version.</param>
        /// <param name="receiver">Receiver address.</param>
        /// <param name="community">Community name.</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        /// <param name="variables">Variables.</param>
        public GetResponseMessage(int sequenceNumber, VersionCode version, IPAddress receiver, OctetString community, IList<Variable> variables)
        {
            _version = version;
            _receiver = receiver;
            _community = community;
            _variables = variables;
            GetResponsePdu pdu = new GetResponsePdu(
                new Integer32(sequenceNumber),
                ErrorCode.NoError,
                new Integer32(0),
                _variables);
            _sequenceNumber = sequenceNumber;
            _bytes = pdu.ToMessageBody(_version, _community).ToBytes();
        }
                
        /// <summary>
        /// Creates a <see cref="GetResponseMessage"/> with a specific <see cref="Sequence"/>.
        /// </summary>
        /// <param name="body">Message body</param>
        public GetResponseMessage(Sequence body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            
            if (body.Items.Count != 3)
            {
                throw new ArgumentException("wrong message body");
            }
            
            _community = (OctetString)body.Items[1];
            _version = (VersionCode)((Integer32)body.Items[0]).ToInt32();    
            _pdu = (ISnmpPdu)body.Items[2];
            if (_pdu.TypeCode != TypeCode)
            {
                throw new ArgumentException("wrong message type");
            }
            
            GetResponsePdu pdu = (GetResponsePdu)_pdu;
            _sequenceNumber = pdu.SequenceNumber;
            _errorStatus = pdu.ErrorStatus;
            _errorIndex = pdu.ErrorIndex;
            _variables = _pdu.Variables;
            _bytes = body.ToBytes();
        }
        
        /// <summary>
        /// Sends this <see cref="GetRequestMessage"/> and handles the response from agent.
        /// </summary>
        /// <param name="port">Port number.</param>
        /// <returns></returns>
        public void Send(int port)
        {
            byte[] bytes = _bytes;
            IPEndPoint receiver = new IPEndPoint(_receiver, port);
            using (UdpClient udp = new UdpClient())
            {
                udp.Send(bytes, bytes.Length, receiver);
                udp.Close();
            }
        }
        
        /// <summary>
        /// Error status.
        /// </summary>
        public ErrorCode ErrorStatus
        {
            get
            {
                return _errorStatus;
            }
        }
        
        /// <summary>
        /// Error index.
        /// </summary>
        public int ErrorIndex
        {
            get
            {
                return _errorIndex;
            }
        }
        
        internal int SequenceNumber
        {
            get
            {
                return _sequenceNumber;
            }
        }
        
        /// <summary>
        /// Variables.
        /// </summary>
        public IList<Variable> Variables
        {
            get
            {
                return _variables;
            }
        }
        
        /// <summary>
        /// PDU.
        /// </summary>
        public ISnmpPdu Pdu 
        {
            get 
            {
                return _pdu;
            }
        }
        
        /// <summary>
        /// Type code
        /// </summary>
        public SnmpType TypeCode 
        {
            get
            {
                return SnmpType.GetResponsePdu;
            }
        }
        
        /// <summary>
        /// Converts to byte format.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return _bytes;
        }
        
        /// <summary>
        /// Returns a <see cref="String"/> that represents this <see cref="GetResponseMessage"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GET response message: version: " + _version + "; " + _community + "; " + _pdu;
        }
    }
}