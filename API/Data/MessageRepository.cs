using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
            .Include(s => s.Sender)
            .Include(r => r.Recipient)
            .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDTO>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages
            .OrderByDescending(m => m.MessageSent)
            .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
            .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && !u.RecipientDeleted),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && !u.SenderDeleted),
                _ => query.Where(u => (u.RecipientUsername == messageParams.Username && u.MessageRead == null
                && !u.RecipientDeleted
                ))
            };
            return await PagedList<MessageDTO>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Where(m =>
                    m.Recipient.UserName == currentUsername && m.RecipientDeleted ==false
                    && m.Sender.UserName == recipientUsername
                    || m.Recipient.UserName ==recipientUsername
                    && m.Sender.UserName == currentUsername && m.SenderDeleted == false
                )
                .MarkUnreadAsRead(currentUsername)
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return messages;
        }
     

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
            .Include(c => c.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
            .Include(c => c.Connections)
            .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
        }
    }
}