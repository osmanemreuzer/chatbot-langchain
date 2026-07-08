import { useState, useEffect } from "react";
import type { User } from "../types";

let globalUser: User | null = null;
let listeners: Array<() => void> = [];

const notify = () => listeners.forEach((fn) => fn());

export const authStore = {
  getUser: () => globalUser,
  getToken: () => localStorage.getItem("token"),

  setAuth: (token: string, user: User) => {
    localStorage.setItem("token", token);
    globalUser = user;
    notify();
  },

  logout: () => {
    localStorage.removeItem("token");
    globalUser = null;
    notify();
  },

  isAuthenticated: () => !!localStorage.getItem("token"),
};

export const useAuth = () => {
  const [user, setUser] = useState<User | null>(globalUser);

  useEffect(() => {
    const update = () => setUser(globalUser);
    listeners.push(update);
    return () => {
      listeners = listeners.filter((fn) => fn !== update);
    };
  }, []);

  return { user, isAuthenticated: authStore.isAuthenticated() };
};
