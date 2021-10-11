using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentNavigation.ViewModels;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.ContentNavigation.Drivers
{
    public class ListPartGroupDisplayDriver : ContentDisplayDriver {
		private readonly ISiteService _siteService;
		private readonly YesSql.ISession _session;
		private readonly IContentManager _contentManager;
		private readonly IContentItemDisplayManager _contentItemDisplayManager;
		private readonly IContentDefinitionManager _contentDefinitionManager;
		private readonly IUpdateModelAccessor _updateModelAccessor;
		private readonly IDisplayManager<ContentOptionsViewModel> _contentOptionsDisplayManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IContentsAdminListQueryService _contentsAdminListQueryService;

		public ListPartGroupDisplayDriver(
			ISiteService siteService,
			IContentManager contentManager,
			IContentDefinitionManager contentDefinitionManager,
			YesSql.ISession session,
			IUpdateModelAccessor updateModelAccessor,
			IHttpContextAccessor httpContextAccessor,
			IDisplayManager<ContentOptionsViewModel> contentOptionsDisplayManager,
			IContentsAdminListQueryService contentsAdminListQueryService
			) {
			_siteService = siteService;
			_contentManager = contentManager;
			_contentDefinitionManager = contentDefinitionManager;
			_session = session;
			_updateModelAccessor = updateModelAccessor;
			_contentOptionsDisplayManager = contentOptionsDisplayManager;
			_httpContextAccessor = httpContextAccessor;
			_contentsAdminListQueryService = contentsAdminListQueryService;
		}

		public override async Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context) {
			string prefix = "List-";
			var results = new List<IDisplayResult>();
            if (context.GroupId.StartsWith(prefix))
            {
                var contentTypeId = context.GroupId.Substring(prefix.Length);
                var siteSettings = await _siteService.GetSiteSettingsAsync();

                var listPartFilterViewModel = new ListPartFilterViewModel();
                results.Add(Initialize<ListGroupViewModel>("ListPartGroup", async model =>
                {
                    var options = new ContentOptionsViewModel();
                    await context.Updater.TryUpdateModelAsync(options,"Options");

                    var pagerParameters = new PagerParameters();
                    await context.Updater.TryUpdateModelAsync(pagerParameters);
                    var pager = new Pager(pagerParameters, siteSettings.PageSize);

                    if (!String.IsNullOrEmpty(contentTypeId))
                    {
                        options.SelectedContentType = contentTypeId;
                    }

					options.ContentSorts = new List<SelectListItem>();

					options.ContentsBulkAction = new List<SelectListItem>();

					options.SearchText = options.FilterResult.ToString();
					options.OriginalSearchText = options.SearchText;
					options.RouteValues.TryAdd("Options.SearchText", options.FilterResult.ToString());
					var routeData = new RouteData(options.RouteValues);
                    var maxPagedCount = siteSettings.MaxPagedCount;
                    if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                    {
                        pager.PageSize = maxPagedCount;
                    }
					var query = await _contentsAdminListQueryService.QueryAsync(options, _updateModelAccessor.ModelUpdater);
					query = query.With<ContainedPartIndex>(a => a.ListContentItemId == contentItem.ContentItemId)
					.With<ContentItemIndex>(a=>a.ContentType == contentTypeId);
                    
					var pagerShape = (await context.New.Pager(pager)).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync()).RouteData(routeData);
					var pageOfContentItems = (await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync(_contentManager)).ToList();
					var startIndex = (pagerShape.Page - 1) * (pagerShape.PageSize) + 1;
					options.StartIndex = startIndex;
					options.EndIndex = startIndex + pageOfContentItems.Count - 1;
					options.ContentItemsCount = pageOfContentItems.Count;
					options.TotalItemCount = pagerShape.TotalItemCount;

					options.ContentItemsCount = pageOfContentItems.Count;
					options.TotalItemCount = pagerShape.TotalItemCount;

					var header = await _contentOptionsDisplayManager.BuildEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

                    model.ContentItem = contentItem;
                    model.ContentItems = pageOfContentItems;
                    model.ContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);
                    model.Context = context;
                    model.Pager = pagerShape;
                    model.Options = options;
                    model.Header = header;

                }).Displaying(ctx =>
                {
                    ctx.Shape.Metadata.Alternates.Add($"ListPartGroup__{contentTypeId}");
                })
                .Location("DetailAdmin", "Content:10").OnGroup(context.GroupId));
            }
            return Combine(results);
		}
	}
}
