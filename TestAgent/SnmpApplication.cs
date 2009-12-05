﻿using System.Collections.Generic;
using Lextm.SharpSnmpLib.Messaging;

namespace Lextm.SharpSnmpLib.Agent
{
    internal class SnmpApplication
    {
        private readonly SnmpContext _context;
        private bool _finished;
        private readonly Logger _logger;
        private readonly SecurityGuard _guard = new SecurityGuard(VersionCode.V1, new OctetString("public"), new OctetString("public"));
        private readonly MessageHandlerFactory _factory;
        private IMessageHandler _handler;
        private const int MaxResponseSize = 1500;
        private readonly ObjectStore _store;

        public SnmpApplication(SnmpContext context, Logger logger, ObjectStore store)
        {
            _context = context;
            _logger = logger;
            _store = store;
            _factory = new MessageHandlerFactory(_store);
        }

        public SnmpContext Context
        {
            get { return _context; }
        }

        public bool RequestFinished
        {
            get { return _finished; }
        }

        public void Process()
        {
            OnAuthenticateRequest();
            OnMapRequestHandler();
            OnRequestHandlerExecute();
            OnLogRequest();
        }

        private void OnRequestHandlerExecute()
        {
            if (RequestFinished)
            {
                return;
            }

            IList<Variable> result = _handler.Handle(Context.Request);

            GetResponseMessage response;
            if (_handler.ErrorStatus == ErrorCode.NoError)
            {
                response = new GetResponseMessage(Context.Request.RequestId, Context.Request.Version, Context.Request.Parameters.UserName,
                                                  ErrorCode.NoError, 0, result);
                if (response.ToBytes().Length > MaxResponseSize)
                {
                    response = new GetResponseMessage(Context.Request.RequestId, Context.Request.Version,
                                                      Context.Request.Parameters.UserName,
                                                      ErrorCode.TooBig, 0, Context.Request.Pdu.Variables);
                }
            }
            else
            {
                response = new GetResponseMessage(Context.Request.RequestId, Context.Request.Version,
                                                  Context.Request.Parameters.UserName,
                                                  _handler.ErrorStatus, _handler.ErrorIndex,
                                                  Context.Request.Pdu.Variables);
            }

            Context.Respond(response);
        }

        private void OnMapRequestHandler()
        {
            if (RequestFinished)
            {
                return;
            }

            _handler = _factory.GetHandler(Context.Request);
            if (_handler == null)
            {
                // TODO: handle error here.
                CompleteRequest();
            }
        }

        private void OnAuthenticateRequest()
        {
            if (!_guard.Allow(Context.Request))
            {
                // TODO: handle error here.
                // return TRAP saying authenticationFailed.
                CompleteRequest();
            }
        }

        private void OnLogRequest()
        {
            _logger.Log(Context);
        }

        public void CompleteRequest()
        {
            _finished = true;
        }
    }
}