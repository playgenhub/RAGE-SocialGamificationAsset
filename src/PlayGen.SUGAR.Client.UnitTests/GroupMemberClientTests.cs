﻿using System;
using System.Linq;
using System.Net;
using PlayGen.SUGAR.Contracts;
using Xunit;

namespace PlayGen.SUGAR.Client.IntegrationTests
{
	public class GroupMemberClientTests
	{
		#region Configuration
		private readonly GroupMemberClient _groupMemberClient;
		private readonly GroupClient _groupClient;
		private readonly UserClient _userClient;

		public GroupMemberClientTests()
		{
			var testSugarClient = new TestSUGARClient();
			_groupMemberClient = testSugarClient.GroupMember;
			_groupClient = testSugarClient.Group;
			_userClient = testSugarClient.User;

			RegisterAndLogin(testSugarClient.Account);
		}

		private void RegisterAndLogin(AccountClient client)
		{
			var accountRequest = new AccountRequest
			{
				Name = "GroupMemberClientTests",
				Password = "GroupMemberClientTestsPassword",
				AutoLogin = true,
			};

			try
			{
				client.Login(accountRequest);
			}
			catch
			{
				client.Register(accountRequest);
			}
		}
		#endregion

		#region Tests
		[Fact]
		public void CanCreateRequest()
		{
			var requestor = GetOrCreateUser("CanCreateRequest");
			var acceptor = GetOrCreateGroup("CanCreateRequest");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			Assert.Equal(relationshipRequest.RequestorId, relationshipResponse.RequestorId);
			Assert.Equal(relationshipRequest.AcceptorId, relationshipResponse.AcceptorId);

			var sent = _groupMemberClient.GetSentRequests(requestor.Id);

			Assert.Equal(1, sent.Count());

			var received = _groupMemberClient.GetMemberRequests(acceptor.Id);

			Assert.Equal(1, received.Count());
		}

		[Fact]
		public void CanCreateAutoAcceptedRequest()
		{
			var requestor = GetOrCreateUser("CanCreateAutoAcceptedRequest");
			var acceptor = GetOrCreateGroup("CanCreateAutoAcceptedRequest");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
				AutoAccept = true,
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			Assert.Equal(relationshipRequest.RequestorId, relationshipResponse.RequestorId);
			Assert.Equal(relationshipRequest.AcceptorId, relationshipResponse.AcceptorId);

			var sent = _groupMemberClient.GetUserGroups(requestor.Id);

			Assert.Equal(1, sent.Count());

			var received = _groupMemberClient.GetMembers(acceptor.Id);

			Assert.Equal(1, received.Count());
		}

		[Fact]
		public void CannotCreateDuplicateRequest()
		{
			var requestor = GetOrCreateUser("CannotCreateDuplicateRequest");
			var acceptor = GetOrCreateGroup("CannotCreateDuplicateRequest");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			Assert.Throws<Exception>(() => _groupMemberClient.CreateMemberRequest(relationshipRequest));
		}

		[Fact]
		public void CannotCreateDuplicateRequestOfAccepted()
		{
			var requestor = GetOrCreateUser("DuplicateRequestOfAccepted");
			var acceptor = GetOrCreateGroup("DuplicateRequestOfAccepted");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
				AutoAccept = true
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			relationshipRequest.AutoAccept = false;

			Assert.Throws<Exception>(() => _groupMemberClient.CreateMemberRequest(relationshipRequest));
		}

		[Fact]
		public void CannotCreateDuplicateAutoAcceptedRequest()
		{
			var requestor = GetOrCreateUser("DuplicateAutoAcceptedRequest");
			var acceptor = GetOrCreateGroup("DuplicateAutoAcceptedRequest");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			relationshipRequest.AutoAccept = true;

			Assert.Throws<Exception>(() => _groupMemberClient.CreateMemberRequest(relationshipRequest));
		}

		[Fact]
		public void CannotCreateRequestWithNonExistingUser()
		{
			var acceptor = GetOrCreateGroup("CannotCreateRequestWithNonExistingUser");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = -1,
				AcceptorId = acceptor.Id,
			};

			Assert.Throws<Exception>(() => _groupMemberClient.CreateMemberRequest(relationshipRequest));
		}

		[Fact]
		public void CannotCreateRequestWithNonExistingGroup()
		{
			var requestor = GetOrCreateUser("RequestWithNonExistingGroup");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = -1,
			};

			Assert.Throws<Exception>(() => _groupMemberClient.CreateMemberRequest(relationshipRequest));
		}

		[Fact]
		public void CanAcceptRequest()
		{
			var requestor = GetOrCreateUser("CanAcceptRequest");
			var acceptor = GetOrCreateGroup("CanAcceptRequest");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			var sent = _groupMemberClient.GetSentRequests(requestor.Id);

			Assert.Equal(1, sent.Count());

			var received = _groupMemberClient.GetMemberRequests(acceptor.Id);

			Assert.Equal(1, received.Count());

			var relationshipStatusUpdate = new RelationshipStatusUpdate()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
				Accepted = true
			};

			_groupMemberClient.UpdateMemberRequest(relationshipStatusUpdate);

			sent = _groupMemberClient.GetSentRequests(requestor.Id);

			Assert.Equal(0, sent.Count());

			received = _groupMemberClient.GetMemberRequests(acceptor.Id);

			Assert.Equal(0, received.Count());

			sent = _groupMemberClient.GetUserGroups(requestor.Id);

			Assert.Equal(1, sent.Count());

			received = _groupMemberClient.GetMembers(acceptor.Id);

			Assert.Equal(1, received.Count());
		}

		[Fact]
		public void CanRejectRequest()
		{
			var requestor = GetOrCreateUser("CanRejectRequest");
			var acceptor = GetOrCreateGroup("CanRejectRequest");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			var sent = _groupMemberClient.GetSentRequests(requestor.Id);

			Assert.Equal(1, sent.Count());

			var received = _groupMemberClient.GetMemberRequests(acceptor.Id);

			Assert.Equal(1, received.Count());

			var relationshipStatusUpdate = new RelationshipStatusUpdate()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
				Accepted = false
			};

			_groupMemberClient.UpdateMemberRequest(relationshipStatusUpdate);

			sent = _groupMemberClient.GetSentRequests(requestor.Id);

			Assert.Equal(0, sent.Count());

			received = _groupMemberClient.GetMemberRequests(acceptor.Id);

			Assert.Equal(0, received.Count());

			sent = _groupMemberClient.GetUserGroups(requestor.Id);

			Assert.Equal(0, sent.Count());

			received = _groupMemberClient.GetMembers(acceptor.Id);

			Assert.Equal(0, received.Count());
		}

		[Fact]
		public void CannotUpdateAlreadyAcceptedRequest()
		{
			var requestor = GetOrCreateUser("CannotUpdateAlreadyAcceptedRequest");
			var acceptor = GetOrCreateGroup("CannotUpdateAlreadyAcceptedRequest");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
				AutoAccept = true
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			var relationshipStatusUpdate = new RelationshipStatusUpdate()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
				Accepted = true
			};

			Assert.Throws<Exception>(() => _groupMemberClient.UpdateMemberRequest(relationshipStatusUpdate));
		}

		[Fact]
		public void CannotUpdateNotExistingRequest()
		{
			var relationshipStatusUpdate = new RelationshipStatusUpdate()
			{
				RequestorId = -1,
				AcceptorId = -1,
				Accepted = true
			};

			Assert.Throws<Exception>(() => _groupMemberClient.UpdateMemberRequest(relationshipStatusUpdate));
		}

		[Fact]
		public void CanUpdateRelationship()
		{
			var requestor = GetOrCreateUser("CanUpdateRelationship");
			var acceptor = GetOrCreateGroup("CanUpdateRelationship");

			var relationshipRequest = new RelationshipRequest()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
				AutoAccept = true,
			};

			var relationshipResponse = _groupMemberClient.CreateMemberRequest(relationshipRequest);

			var sent = _groupMemberClient.GetUserGroups(requestor.Id);

			Assert.Equal(1, sent.Count());

			var received = _groupMemberClient.GetMembers(acceptor.Id);

			Assert.Equal(1, received.Count());

			var relationshipStatusUpdate = new RelationshipStatusUpdate()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
			};

			_groupMemberClient.UpdateMember(relationshipStatusUpdate);

			sent = _groupMemberClient.GetUserGroups(requestor.Id);

			Assert.Equal(0, sent.Count());

			received = _groupMemberClient.GetMembers(acceptor.Id);

			Assert.Equal(0, received.Count());
		}

		[Fact]
		public void CannotUpdateNotExistingRelationship()
		{
			var requestor = GetOrCreateUser("CannotUpdateNotExistingRelationship");
			var acceptor = GetOrCreateGroup("CannotUpdateNotExistingRelationship");

			var relationshipStatusUpdate = new RelationshipStatusUpdate()
			{
				RequestorId = requestor.Id,
				AcceptorId = acceptor.Id,
			};

			Assert.Throws<Exception>(() => _groupMemberClient.UpdateMember(relationshipStatusUpdate));
		}
		#endregion

		#region Helpers
		private ActorResponse GetOrCreateUser(string suffix)
		{
			string name = "GroupMemberControllerTests" + suffix ?? $"_{suffix}";
			var users = _userClient.Get(name, true);
			ActorResponse user;

			if (users.Any())
			{
				user = users.Single();
			}
			else
			{
				user = _userClient.Create(new ActorRequest
				{
					Name = name
				});
			}

			return user;
		}

		private ActorResponse GetOrCreateGroup(string suffix)
		{
			string name = "GroupMemberControllerTests" + suffix ?? $"_{suffix}";
			var groups = _groupClient.Get(name);
			ActorResponse group;

			if (groups.Any())
			{
				group = groups.Single();
			}
			else
			{
				group = _groupClient.Create(new ActorRequest
				{
					Name = name
				});
			}

			return group;
		}
		#endregion

	}
}
