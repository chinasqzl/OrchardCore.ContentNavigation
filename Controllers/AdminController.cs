using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentNavigation.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.ContentNavigation.Controllers
{
	public class AdminController : Controller, IUpdateModel {
		private readonly IContentManager _contentManager;
		private readonly IContentItemDisplayManager _contentItemDisplayManager;
		private readonly IUpdateModelAccessor _updateModelAccessor;
		private readonly IAuthorizationService _authorizationService;
		private readonly YesSql.ISession _session;
		private readonly INotifier _notifier;
		private readonly IHtmlLocalizer H;

		public AdminController(
			IContentManager contentManager,
			IContentItemDisplayManager contentItemDisplayManager,
			IUpdateModelAccessor updateModelAccessor,
			IAuthorizationService authorizationService,
			YesSql.ISession session,
			INotifier notifier,
			IHtmlLocalizer<AdminController> htmlLocalizer
			) {
			_contentItemDisplayManager = contentItemDisplayManager;
			_contentManager = contentManager;
			_updateModelAccessor = updateModelAccessor;
			_authorizationService = authorizationService;
			_session = session;
			_notifier = notifier;

			H = htmlLocalizer;
		}

		public IHtmlLocalizer T { get; }
		public ILogger Logger { get; set; }

		public async Task<IActionResult> Display(string contentItemId, string groupId) {
			var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
			if (contentItem == null) {
				return NotFound();
			}
			var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "DetailAdmin", groupId);
			return View(model);
		}

		public async Task<IActionResult> Edit(string contentItemId, string groupId)
		{
			//todo 添加内容分组权限验证
			var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
			if (contentItem == null)
			{
				return NotFound();
			}

			var group = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem)).EditorGroupInfo.FirstOrDefault(g => g.Id == groupId);
			if (group == null)
			{
				return NotFound();
			}

			var viewModel = new ContentGroupViewModel
			{
				GroupId = groupId,
				GroupName = group.Name,
				Shape = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false, groupId)
			};

			return View(viewModel);
		}

		[HttpPost]
		[ActionName(nameof(Edit))]
		public async Task<IActionResult> EditPost(string contentItemId, string groupId)
		{
			var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

			if (contentItem == null)
			{
				return NotFound();
			}

			var group = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem)).EditorGroupInfo.FirstOrDefault(g => g.Id == groupId);
			if (group == null)
			{
				return NotFound();
			}

			//if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
			//{
			//	return Forbid();
			//}

			var viewModel = new ContentGroupViewModel
			{
				GroupId = groupId,
				GroupName = group.Name,
				Shape = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false, groupId)
			};

			if (!ModelState.IsValid)
			{
				await _session.CancelAsync();
				return View("Edit", viewModel);
			}

			await _contentManager.UpdateAsync(contentItem);

			//var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
			//_notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
			//	? H["Your content has been published."]
			//	: H["Your {0} has been published.", typeDefinition.DisplayName]);

			return View(viewModel);
		}
	}
}
