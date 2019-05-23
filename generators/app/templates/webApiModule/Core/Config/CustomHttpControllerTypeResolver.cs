using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;

namespace <%= options.moduleName %>.Core.Config
{
    /// <summary>
    /// Custom resolver used to map http routes to exposed controllers. 
    /// </summary>
    public class CustomHttpControllerTypeResolver : DefaultHttpControllerTypeResolver
    {
        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return base.GetControllerTypes(assembliesResolver);
        }

        /// <summary>
        /// Predicate running for each exposed type. we will map only those extending our custom base controllers
        /// </summary>
        protected override Predicate<Type> IsControllerTypePredicate => type =>
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return type.IsClass
                   && type.IsVisible
                   && !type.IsAbstract
                   && typeof(BaseApiController).IsAssignableFrom(type);
        };
    }
}