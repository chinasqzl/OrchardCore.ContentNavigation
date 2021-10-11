using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentNavigation.ViewModels
{
    public class ListGroupViewModel
	{
		[BindNever]
		public ContentItem ContentItem { get; set; }
		[BindNever]
		public List<ContentItem> ContentItems { get; set; }
		[BindNever]
		public ContentTypeDefinition ContentTypeDefinition { get; set; }
		[BindNever]
		public BuildDisplayContext Context { get; set; }
		[BindNever]
		public dynamic Pager { get; set; }
		[BindNever]
		public dynamic Header { get; set; }

		public ContentOptionsViewModel Options { get; set; }
	}
}
