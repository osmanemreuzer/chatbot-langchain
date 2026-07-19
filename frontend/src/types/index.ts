export interface User {
  id: string;
  email: string;
}

export interface Message {
  id: string;
  role: "user" | "assistant";
  content: string;
  createdAt: string;
}

export interface Conversation {
  id: string;
  title: string;
  createdAt: string;
  updatedAt: string;
}

export interface ConversationDetail extends Conversation {
  messages: Message[];
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface SendMessageRequest {
  conversationId: string;
  content: string;
}

export interface DocumentItem {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  createdAt: string;
}
