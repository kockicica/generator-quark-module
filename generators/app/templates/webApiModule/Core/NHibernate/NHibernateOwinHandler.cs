using System;

using Castle.Core.Logging;

using Microsoft.Owin;

using NHibernate;

using Owin;

namespace <%= options.moduleName %>.Core.NHibernate
{
    public class NHibernateOwinHandler
    {
        private ISessionFactory _sessionFactory;

        public const string NHibernateSessionKey = "nhibernate.session";
        public const string NHibernateSessionFactoryKey = "nhibernate.sessionfactory";


        public NHibernateOwinHandler(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public void BeforeRequest(IOwinContext context)
        {
            // create session
            var session = _sessionFactory.OpenSession();
            //Logger.DebugFormat("Session opened");
            // begin transaction
            session.BeginTransaction();
            // and add session information to owin context
            context.Environment.Add(NHibernateSessionKey, session);

            // add session factory, also
            context.Environment.Add(NHibernateSessionFactoryKey, _sessionFactory);

        }

        public void AfterRequest(IOwinContext context)
        {
            if (context.Environment.ContainsKey(NHibernateSessionKey))
            {
                // if it contains previously opened session
                ISession session = null;
                try
                {
                    // try to commit
                    session = (ISession)context.Environment[NHibernateSessionKey];
                    if (session.Transaction != null)
                    {
                        session.Transaction.Commit();
                    }
                    else
                    {
                        Logger.WarnFormat("No transaction found");
                    }
                    //Logger.DebugFormat("Transaction commited");
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                }
                finally
                {
                    if (session != null)
                    {
                        session.Close();
                        //Logger.DebugFormat("Session closed");
                    }
                }
            }

        }
    }

    /// <summary>
    /// Helper method used for installing NHibernateOwinHandler in WebApiStartup
    /// </summary>
    public static class NHibernateOwinHanderExtensions
    {
        public static void UseNHibernateHandler(this IAppBuilder appBuilder, NHibernateOwinHandler handler)
        {
            appBuilder.Use(async (context, func) =>
            {
                handler.BeforeRequest(context);
                await func();
                handler.AfterRequest(context);
            });
        }
    }

}
