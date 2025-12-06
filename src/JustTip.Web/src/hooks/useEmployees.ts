import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { EmployeeDto } from '../types/employee'

async function fetchEmployees(): Promise<EmployeeDto[]> {
  const response = await api.get<EmployeeDto[]>('/employees')
  return response.data
}

export function useEmployees() {
  return useQuery({
    queryKey: ['employees'],
    queryFn: fetchEmployees,
  })
}
