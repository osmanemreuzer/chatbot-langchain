import client from "./client";
import type { Conversation, ConversationDetail, Message } from "../types";

export const getConversations = () =>
  client.get<Conversation[]>("/conversations");

export const createConversation = () =>
  client.post<Conversation>("/conversations");

export const getConversation = (conversationId: string) =>
  client.get<ConversationDetail>(`/conversations/${conversationId}`);

export const sendMessage = (conversationId: string, content: string) =>
  client.post<Message>(`/conversations/${conversationId}/messages`, {
    role: "user",
    content,
  });

export const deleteConversation = (conversationId: string) =>
  client.delete(`/conversations/${conversationId}`);
