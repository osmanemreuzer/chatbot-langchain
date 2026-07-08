import client from "./client";
import type { Conversation, Message } from "../types";

export const getConversations = () =>
  client.get<Conversation[]>("/conversations");

export const createConversation = () =>
  client.post<Conversation>("/conversations");

export const getMessages = (conversationId: string) =>
  client.get<Message[]>(`/conversations/${conversationId}/messages`);

export const sendMessage = (conversationId: string, content: string) =>
  client.post<Message>(`/conversations/${conversationId}/messages`, { content });

export const deleteConversation = (conversationId: string) =>
  client.delete(`/conversations/${conversationId}`);
