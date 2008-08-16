using System;
using System.Collections.Generic;
using System.Text;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Mib;
using System.Net;

namespace Lextm.SharpSnmpLib.Browser
{
	internal delegate void ReportMessage(string message);
	
	internal class AgentProfile
	{
	    private string _get;
	    private string _set;
	    private VersionCode _version;
        private IPEndPoint _agent;
	
	    internal AgentProfile(VersionCode version, IPAddress ip, int port, string getCommunity, string setCommunity)
	    {
	        _get = getCommunity;
	        _set = setCommunity;
	        _version = version;
            _agent = new IPEndPoint(ip, port);
	    }
	    
	    internal int Port
	    {
	        get { return _agent.Port; }
	    }
	    
	    internal IPAddress IP
	    {
	        get { return _agent.Address; }
	    }

        internal IPEndPoint Agent
        {
            get
            {
                return _agent;
            }
        }

        internal VersionCode VersionCode
        {
            get { return _version; }
        }

        internal string GetCommunity
        {
            get { return _get; }
        }

        internal string SetCommunity
        {
            get { return _set; }
        }
	
	    internal event ReportMessage OnOperationCompleted;
	
	    internal void Get(Manager manager, string textual)
	    {
	        Report(manager.Get(_agent, _get, new Variable(textual)));
	    }
	
	    private void Report(Variable variable)
	    {
	        if (OnOperationCompleted != null)
	        {
	            OnOperationCompleted(variable.ToString());
	        }
	    }
	
	    internal void Set(Manager manager, string textual, ISnmpData data)
	    {
            manager.Set(_agent, _get, new Variable(textual, data));
	    }
	
	    // private IPAddress ValidateIP()
	    // {
            // IPAddress ip;
            // bool succeeded = IsValidIPAddress(_ip, out ip);
	        // if (!succeeded)
	        // {
	            // throw new MibBrowserException("Invalid IP address: " + _ip);
	        // }
	        // return ip;
	    // }

        internal static bool IsValidIPAddress(string address, out IPAddress ip)
        {
            return IPAddress.TryParse(address, out ip);
        }
	
	    internal void Walk(Manager manager, IDefinition def)
	    {
	    }
    }
}