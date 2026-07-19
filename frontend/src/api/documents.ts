import client from "./client";
import type { DocumentItem } from "../types";

export const getDocuments = () =>
  client.get<DocumentItem[]>("/documents");

export const uploadDocument = (file: File) => {
  const formData = new FormData();
  formData.append("file", file);
  return client.post<DocumentItem>("/documents", formData, {
    headers: { "Content-Type": undefined },
  });
};

export const deleteDocument = (documentId: string) =>
  client.delete(`/documents/${documentId}`);
