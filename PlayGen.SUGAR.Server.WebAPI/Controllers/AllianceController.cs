﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayGen.SUGAR.Common;
using PlayGen.SUGAR.Common.Authorization;
using PlayGen.SUGAR.Contracts;
using PlayGen.SUGAR.Server.Authorization;
using PlayGen.SUGAR.Server.WebAPI.Attributes;
using PlayGen.SUGAR.Server.WebAPI.Extensions;

namespace PlayGen.SUGAR.Server.WebAPI.Controllers
{
	/// <summary>
	/// Web Controller that facilitates Group to Group relationship specific operations.
	/// </summary>
	// Values ensured to not be nulled by model validation
	[SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
	[Route("api/[controller]")]
	[Authorize("Bearer")]
	public class AllianceController : Controller
	{
		private readonly IAuthorizationService _authorizationService;
		private readonly Core.Controllers.RelationshipController _relationshipCoreController;

		public AllianceController(Core.Controllers.RelationshipController relationshipController, IAuthorizationService authorizationService)
		{
			_relationshipCoreController = relationshipController;
			_authorizationService = authorizationService;
		}

		/// <summary>
		/// Get a list of all groups that have sent relationship requests to this groupId.
		/// </summary>
		/// <param name="groupId">ID of the group.</param>
		/// <returns>A list of <see cref="ActorResponse"/> which match the search criteria.</returns>
		[HttpGet("requests/{groupId:int}")]
		[Authorization(ClaimScope.Group, AuthorizationAction.Get, AuthorizationEntity.AllianceRequest)]
		public async Task<IActionResult> GetAllianceRequests([FromRoute]int groupId)
		{
			if ((await _authorizationService.AuthorizeAsync(User, groupId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded)
			{
				var groups = _relationshipCoreController.GetRequests(groupId, ActorType.Group);
				var actorContract = groups.ToActorContractList();
				return new ObjectResult(actorContract);
			}
			return Forbid();
		}

		/// <summary>
		/// Get a list of all Groups that have been sent relationship requests from this groupId.
		/// </summary>
		/// <param name="groupId">ID of the group.</param>
		/// <returns>A list of <see cref="ActorResponse"/> which match the search criteria.</returns>
		[HttpGet("sentrequests/{groupId:int}")]
		[Authorization(ClaimScope.Group, AuthorizationAction.Get, AuthorizationEntity.AllianceRequest)]
		public async Task<IActionResult> GetSentRequests([FromRoute]int groupId)
		{
			if ((await _authorizationService.AuthorizeAsync(User, groupId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded)
			{
				var requests = _relationshipCoreController.GetSentRequests(groupId, ActorType.Group);
				var actorContract = requests.ToActorContractList();
				return new ObjectResult(actorContract);
			}
			return Forbid();
		}

		/// <summary>
		/// Get a list of all groups that have relationships with this groupId.
		/// </summary>
		/// <param name="groupId">ID of the group.</param>
		/// <returns>A list of <see cref="ActorResponse"/> which match the search criteria.</returns>
		[HttpGet("{groupId:int}")]
		public IActionResult GetAlliances([FromRoute]int groupId)
		{
			var members = _relationshipCoreController.GetRelatedActors(groupId, ActorType.Group);
			var actorContract = members.ToActorContractList();
			return new ObjectResult(actorContract);
		}

		/// <summary>
		/// Get a count of groups that have a relationship with this groupId.
		/// </summary>
		/// <param name="groupId">ID of the group.</param>
		/// <returns>A count of alliances for the group that matches the search criteria.</returns>
		[HttpGet("count/{groupId:int}")]
		public IActionResult GetAllianceCount([FromRoute]int groupId)
		{
			var count = _relationshipCoreController.GetRelationshipCount(groupId, ActorType.Group);
			return new ObjectResult(count);
		}

		/// <summary>
		/// Create a new relationship request between a Group and a Group.
		/// Requires a relationship between the Groups to not already exist.
		/// </summary>
		/// <param name="relationship"><see cref="RelationshipRequest"/> object that holds the details of the new relationship request.</param>
		/// <returns>A <see cref="RelationshipResponse"/> containing the new Relationship details.</returns>
		[HttpPost]
		[ArgumentsNotNull]
		[Authorization(ClaimScope.Group, AuthorizationAction.Create, AuthorizationEntity.AllianceRequest)]
		public async Task<IActionResult> CreateAllianceRequest([FromBody]RelationshipRequest relationship)
		{
			if ((await _authorizationService.AuthorizeAsync(User, relationship.RequestorId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded ||
				(await _authorizationService.AuthorizeAsync(User, relationship.AcceptorId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded)
			{
				var request = relationship.ToRelationshipModel();
				_relationshipCoreController.CreateRequest(relationship.ToRelationshipModel(), relationship.AutoAccept);
				var relationshipContract = request.ToContract();
				return new ObjectResult(relationshipContract);
			}
			return Forbid();
		}

		/// <summary>
		/// Update an existing relationship request between two Groups.
		/// Requires the relationship request to already exist between the two Groups.
		/// </summary>
		/// <param name="relationship"><see cref="RelationshipStatusUpdate"/> object that holds the details of the relationship.</param>
		[HttpPut("request")]
		[ArgumentsNotNull]
		[Authorization(ClaimScope.Group, AuthorizationAction.Update, AuthorizationEntity.AllianceRequest)]
		public async Task<IActionResult> UpdateAllianceRequest([FromBody] RelationshipStatusUpdate relationship)
		{
			if ((await _authorizationService.AuthorizeAsync(User, relationship.RequestorId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded ||
				(await _authorizationService.AuthorizeAsync(User, relationship.AcceptorId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded)
			{
				var relation = new RelationshipRequest
				{
					RequestorId = relationship.RequestorId,
					AcceptorId = relationship.AcceptorId
				};
				_relationshipCoreController.UpdateRequest(relation.ToRelationshipModel(), relationship.Accepted);
				return Ok();
			}
			return Forbid();
		}

        /// <summary>
        /// Update an existing relationship between two Groups.
        /// Requires the relationship to already exist between the two Groups.
        /// </summary>
        /// <param name="relationship"><see cref="RelationshipStatusUpdate"/> object that holds the details of the relationship.</param>
		[HttpPut] // todo change to a remove that takes both members of the relationship
        [ArgumentsNotNull]
		[Authorization(ClaimScope.Group, AuthorizationAction.Delete, AuthorizationEntity.Alliance)]
		public async Task<IActionResult> RemoveAlliance([FromBody] RelationshipStatusUpdate relationship)
		{
			if ((await _authorizationService.AuthorizeAsync(User, relationship.RequestorId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded ||
				(await _authorizationService.AuthorizeAsync(User, relationship.AcceptorId, HttpContext.ScopeItems(ClaimScope.Group))).Succeeded)
			{
				var relation = new RelationshipRequest
				{
					RequestorId = relationship.RequestorId,
					AcceptorId = relationship.AcceptorId
				};
				_relationshipCoreController.Delete(relation.ToRelationshipModel());
				return Ok();
			}
			return Forbid();
		}
	}
}